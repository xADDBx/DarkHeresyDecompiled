using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimancerComponent))]
public class AnimationManager : MonoBehaviour, IAnimationManager, IInterpolatable
{
	[NonSerialized]
	public bool IsInDollRoom;

	private string m_GameObjectName;

	private bool m_IsInitialized;

	private Animator m_Animator;

	private AnimancerComponent m_Animancer;

	private readonly List<AnimancerLayer> m_AnimancerLayers = new List<AnimancerLayer>();

	private DirectionalMixerState m_LocomotionMixerState;

	private float m_DefaultSpeed = 1f;

	private AnimationActionHandle m_CurrentAction;

	private readonly List<AnimationActionHandle> m_ActiveActions = new List<AnimationActionHandle>();

	private readonly Queue<AnimationActionHandle> m_SequencedActions = new Queue<AnimationActionHandle>();

	private readonly List<AnimationBase> m_ActiveAnimations = new List<AnimationBase>();

	private readonly Dictionary<AnimationActionBase, List<Transition>> m_FromTransitions = new Dictionary<AnimationActionBase, List<Transition>>();

	private readonly Dictionary<AnimationActionBase, List<Transition>> m_ToTransitions = new Dictionary<AnimationActionBase, List<Transition>>();

	private CountableFlag m_RotationForbidden = new CountableFlag();

	private readonly CountingGuard m_Disabled = new CountingGuard();

	private int m_LastUpdateTick;

	[SerializeField]
	private AnimationSet m_AnimationSet;

	[SerializeField]
	private bool m_FireEvents = true;

	public StatefulRandom StatefulRandom
	{
		get
		{
			if (!IsInDollRoom)
			{
				return PFStatefulRandom.Visuals.Animation3;
			}
			return PFStatefulRandom.Visuals.DollRoom;
		}
	}

	private AnimancerLayer m_LocomotionLayer => m_AnimancerLayers[0];

	public bool IsValid => m_Animancer.Graph.PlayableGraph.IsValid();

	public DirectorUpdateMode UpdateMode
	{
		get
		{
			return m_Animancer.Graph.UpdateMode;
		}
		set
		{
			m_Animancer.Graph.UpdateMode = value;
			if (value != DirectorUpdateMode.Manual)
			{
				m_Animancer.Graph.PlayableGraph.Play();
			}
		}
	}

	public float PlayingSpeed => DefaultSpeed;

	public UnitAnimationCallbackReceiver CallbackReceiver => GetComponent<UnitAnimationCallbackReceiver>();

	private static int CurrentUpdateTick => Game.Instance.RealTimeController.CurrentSystemStepIndex;

	public bool IsRotationForbidden => m_RotationForbidden;

	public float DefaultSpeed
	{
		get
		{
			if (!Disabled)
			{
				return m_DefaultSpeed;
			}
			return 0f;
		}
		set
		{
			m_DefaultSpeed = value;
			if (!Disabled)
			{
				m_Animancer.Graph.Speed = value;
			}
		}
	}

	public bool Disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled.Value = value;
			m_Animancer.Graph.Speed = DefaultSpeed;
		}
	}

	public Animator Animator => m_Animator;

	public IReadOnlyList<AnimationActionHandle> ActiveActions => m_ActiveActions;

	public List<AnimationBase> ActiveAnimations => m_ActiveAnimations;

	public Queue<AnimationActionHandle> SequencedActions => m_SequencedActions;

	public AnimationActionHandle CurrentAction => m_CurrentAction;

	public AnimationSet AnimationSet
	{
		get
		{
			if (m_AnimationSet == null)
			{
				return ConfigRoot.Instance.SystemMechanics.HumanAnimationSet;
			}
			return m_AnimationSet;
		}
		set
		{
			m_AnimationSet = value;
			if (Application.isPlaying)
			{
				RuntimeInitializeAnimationSet();
				OnAnimationSetChanged();
			}
		}
	}

	public bool FireEvents
	{
		get
		{
			return m_FireEvents;
		}
		set
		{
			m_FireEvents = value;
		}
	}

	private void Awake()
	{
		m_GameObjectName = base.gameObject.name;
	}

	protected virtual void OnEnable()
	{
		Game.Instance.Controllers.AnimationManagerController.Subscribe(this);
		Game.Instance.Controllers.InterpolationController.Add(this);
		if (!m_IsInitialized)
		{
			m_Animator = GetComponent<Animator>();
			m_Animancer = GetComponent<AnimancerComponent>();
			m_Animancer.ActionOnDisable = AnimancerComponent.DisableAction.Pause;
			CreateLayers();
			RuntimeInitializeAnimationSet();
			m_IsInitialized = true;
		}
	}

	private void CreateLayers()
	{
		int num = 11;
		for (int i = 0; i < num; i++)
		{
			AnimancerLayer item = m_Animancer.Layers[i];
			m_AnimancerLayers.Add(item);
		}
		m_LocomotionLayer.SetWeight(1f);
		m_LocomotionMixerState = new DirectionalMixerState();
		m_LocomotionLayer.Play(m_LocomotionMixerState);
		m_AnimancerLayers[4].IsAdditive = true;
		m_AnimancerLayers[9].IsAdditive = true;
	}

	protected virtual void OnDisable()
	{
		Game.Instance.Controllers.AnimationManagerController.Unsubscribe(this);
		Game.Instance.Controllers.InterpolationController.Remove(this);
	}

	public void CustomUpdate(float dt)
	{
		using (Counters.AnimationManager?.Measure())
		{
			if (!Disabled)
			{
				using (ProfileScope.New("AnimationManager.UpdateAnimations"))
				{
					UpdateAnimations(dt);
				}
				using (ProfileScope.New("AnimationManager.UpdateActions"))
				{
					UpdateActions(dt);
				}
				m_Animator.fireEvents = FireEvents && m_CurrentAction?.ActiveAnimation?.AnimatorController != null;
				m_LastUpdateTick = CurrentUpdateTick;
			}
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		if (m_LastUpdateTick != CurrentUpdateTick || Disabled)
		{
			return;
		}
		Game instance = Game.Instance;
		if (instance != null && instance.IsPaused)
		{
			return;
		}
		using (ProfileScope.New("AnimationManager.InterpolateAnimations"))
		{
			InterpolateAnimations(progress);
		}
	}

	protected virtual void UpdateAnimations(float dt)
	{
		if (UpdateMode == DirectorUpdateMode.Manual)
		{
			m_Animancer.Evaluate(dt);
		}
		AnimationBase animationBase = m_CurrentAction?.ActiveAnimation;
		animationBase?.Update(dt);
		for (int i = 0; i < m_ActiveAnimations.Count; i++)
		{
			AnimationBase animationBase2 = m_ActiveAnimations[i];
			if (animationBase2 != animationBase)
			{
				animationBase2.Update(dt);
			}
			if (animationBase2.State == AnimationState.Finished)
			{
				using (ProfileScope.New("Finishing"))
				{
					m_ActiveAnimations.RemoveAt(i);
					if (animationBase2.Handle.ActiveAnimation == animationBase2)
					{
						StartFadeOutAnimation(animationBase2);
						animationBase2.Handle.ActiveAnimation = null;
					}
					animationBase2.RemoveFromManager();
					i--;
				}
			}
			else if (animationBase2.Handle.ActiveAnimation != animationBase2)
			{
				m_ActiveAnimations.RemoveAt(i);
				animationBase2.RemoveFromManager();
				i--;
			}
		}
		InterpolateAnimations(0f, force: true);
	}

	private void StartFadeOutAnimation(AnimationBase animation)
	{
		if (m_ActiveAnimations.Any((AnimationBase a) => a.GetActiveClip() == animation.GetActiveClip() && a.Handle.AnimationLayer == animation.Handle.AnimationLayer))
		{
			return;
		}
		AnimancerLayer animancerLayer = m_AnimancerLayers[(int)animation.Handle.AnimationLayer];
		AnimancerState animancerState = animancerLayer.ActiveStates.FirstOrDefault((AnimancerState s) => s.Clip == animation.GetActiveClip());
		if (animancerState == null)
		{
			return;
		}
		float transitionOutDuration = GetTransitionOutDuration(animation.Handle);
		if (animancerLayer.ActiveStates.Count > 1 && animancerLayer.CurrentState != animancerState)
		{
			if (animancerState.FadeGroup == null || animancerState.TargetWeight != 0f)
			{
				float targetWeight = animancerLayer.CurrentState.TargetWeight;
				float fadeDuration = animancerLayer.CurrentState.FadeGroup.FadeDuration;
				animancerState.FadeGroup?.Cancel();
				animancerState.StartFade(0f, transitionOutDuration);
				animancerLayer.CurrentState.FadeGroup?.Cancel();
				animancerLayer.CurrentState.StartFade(targetWeight, fadeDuration);
			}
		}
		else
		{
			animancerLayer.StartFade(0f, transitionOutDuration);
		}
	}

	private void InterpolateAnimations(float progress, bool force = false)
	{
		float num = 1f;
		AnimationBase animationBase = m_CurrentAction?.ActiveAnimation;
		if (animationBase != null)
		{
			animationBase.Interpolate(progress, 1f, force);
			if (animationBase.State != AnimationState.Finished && !animationBase.DoNotZeroOtherAnimations)
			{
				using (ProfileScope.New("GetWeight"))
				{
					num = 1f - m_CurrentAction.ActiveAnimation.GetWeight();
				}
			}
		}
		int num2 = 0;
		foreach (AnimationBase activeAnimation in m_ActiveAnimations)
		{
			if (activeAnimation.Handle.HasCrossfadePriority)
			{
				num2++;
			}
		}
		bool flag = num2 >= 2;
		foreach (AnimationBase activeAnimation2 in m_ActiveAnimations)
		{
			if (activeAnimation2 != animationBase)
			{
				float weightFromManager = ((activeAnimation2.Handle == m_CurrentAction || activeAnimation2.Handle.IsAdditive || (flag && activeAnimation2.Handle.HasCrossfadePriority)) ? 1f : ((flag && !activeAnimation2.Handle.HasCrossfadePriority) ? 0f : num));
				activeAnimation2.Interpolate(progress, weightFromManager, force);
			}
		}
	}

	protected virtual void OnAnimationSetChanged()
	{
	}

	private void UpdateActions(float dt)
	{
		for (int i = 0; i < m_ActiveActions.Count; i++)
		{
			AnimationActionHandle animationActionHandle = m_ActiveActions[i];
			if (animationActionHandle.IsStarted)
			{
				animationActionHandle.UpdateInternal(dt);
				if (animationActionHandle.IsReleased && animationActionHandle.ActiveAnimation == null)
				{
					animationActionHandle.FinishInternal();
				}
				if (animationActionHandle.IsFinished)
				{
					if (animationActionHandle == m_CurrentAction)
					{
						m_CurrentAction = null;
					}
					RemoveActionHandleAt(i);
					i--;
				}
			}
			else
			{
				animationActionHandle.StartInternal();
			}
		}
		if (m_CurrentAction == null)
		{
			double num = double.MinValue;
			for (int j = 0; j < m_ActiveActions.Count; j++)
			{
				AnimationActionHandle animationActionHandle2 = m_ActiveActions[j];
				if (animationActionHandle2.DontReleaseOnInterrupt && animationActionHandle2.ActiveAnimation != null && animationActionHandle2.ActiveAnimation.CreationTime.TotalSeconds > num)
				{
					num = animationActionHandle2.ActiveAnimation.CreationTime.TotalSeconds;
					m_CurrentAction = animationActionHandle2;
				}
			}
		}
		if ((m_CurrentAction == null || m_CurrentAction.IsReleased || m_CurrentAction.DontReleaseOnInterrupt) && m_SequencedActions.Count > 0)
		{
			m_CurrentAction = m_SequencedActions.Dequeue();
			AddActionHandle(m_CurrentAction);
			m_CurrentAction.StartInternal();
		}
	}

	protected void AddActionHandle(AnimationActionHandle handle)
	{
		m_ActiveActions.Add(handle);
		if (handle.PreventsRotation)
		{
			m_RotationForbidden.Retain();
		}
	}

	protected void RemoveActionHandle(AnimationActionHandle handle)
	{
		m_ActiveActions.Remove(handle);
		if (handle.PreventsRotation)
		{
			m_RotationForbidden.Release();
		}
	}

	protected void RemoveActionHandleAt(int index)
	{
		AnimationActionHandle animationActionHandle = m_ActiveActions[index];
		m_ActiveActions.RemoveAt(index);
		if (animationActionHandle.PreventsRotation)
		{
			m_RotationForbidden.Release();
		}
	}

	public void StartEvents()
	{
	}

	public void SuspendEvents()
	{
	}

	public void StopEvents()
	{
	}

	private void RuntimeInitializeAnimationSet()
	{
		ResetTransitionsAndActions();
		if (m_AnimationSet == null)
		{
			return;
		}
		foreach (AnimationActionBase action in m_AnimationSet.Actions.Where((AnimationActionBase a) => a))
		{
			List<Transition> value = m_AnimationSet.Transitions.Where((Transition t) => t.FromAction == action).ToList();
			List<Transition> value2 = m_AnimationSet.Transitions.Where((Transition t) => t.ToAction == action).ToList();
			m_FromTransitions[action] = value;
			m_ToTransitions[action] = value2;
			if (action is UnitAnimationActionLocomotion unitAnimationActionLocomotion)
			{
				unitAnimationActionLocomotion.PreloadWeaponStyles();
			}
		}
		if (m_AnimationSet.StartupAction != null)
		{
			Execute(m_AnimationSet.StartupAction);
		}
	}

	protected void ResetTransitionsAndActions()
	{
		m_FromTransitions.Clear();
		m_ToTransitions.Clear();
		m_SequencedActions.Clear();
		foreach (AnimationActionHandle activeAction in m_ActiveActions)
		{
			activeAction.FinishInternal();
		}
		foreach (AnimationActionHandle activeAction2 in m_ActiveActions)
		{
			if (activeAction2.PreventsRotation)
			{
				m_RotationForbidden.Release();
			}
		}
		m_ActiveActions.Clear();
		m_ActiveAnimations.Clear();
		foreach (AnimancerLayer layer in m_Animancer.Layers)
		{
			layer.StartFade(0f, 0.2f);
		}
	}

	public void PrepareForCombat()
	{
		foreach (AnimationActionHandle item in m_ActiveActions.Where((AnimationActionHandle h) => h.Action.ForceFinishOnJoinCombat))
		{
			item.FinishInternal();
		}
	}

	public virtual AnimationActionHandle CreateHandle([NotNull] AnimationActionBase animationAction)
	{
		return new AnimationActionHandle(animationAction, this);
	}

	public virtual void Execute(AnimationActionHandle handle)
	{
		if (handle == null)
		{
			PFLog.Animations.Error("AnimationActionHandle is null");
			return;
		}
		using (ProfileScope.New("Animator.Execute " + handle.Action.name))
		{
			if (handle.Manager != this)
			{
				PFLog.Animations.Error("Can't execute handle which created by another manager.");
			}
			else if (handle.IsStarted)
			{
				PFLog.Animations.Error("Started animation action handle can't be executed multiple times.");
			}
			else if (m_ActiveActions.Contains(handle))
			{
				PFLog.Animations.Error("Action handle already added to manager");
			}
			else if (handle.IsAdditive)
			{
				if (!handle.Action.IsAdditiveToItself)
				{
					foreach (AnimationActionHandle activeAction in ActiveActions)
					{
						if (handle.Action.GetType() == activeAction.Action.GetType() && !activeAction.IsReleased)
						{
							return;
						}
					}
				}
				if (handle.Action.IsAdditiveInterruptsSameType)
				{
					if (m_CurrentAction != null && !m_CurrentAction.DontReleaseOnInterrupt && handle.Action.GetType() == m_CurrentAction.Action.GetType())
					{
						m_CurrentAction.Release();
					}
					foreach (AnimationActionHandle activeAction2 in m_ActiveActions)
					{
						if (!activeAction2.DontReleaseOnInterrupt && handle.Action.GetType() == activeAction2.Action.GetType())
						{
							activeAction2.Release();
						}
					}
				}
				AddActionHandle(handle);
				handle.StartInternal();
			}
			else if (handle.Action.ExecutionMode == ExecutionMode.Interrupted)
			{
				AddActionHandle(handle);
				AnimationActionHandle currentAction = m_CurrentAction;
				if (currentAction != null && !currentAction.DontReleaseOnInterrupt && (!(m_CurrentAction.Action is WarhammerUnitAnimationActionHandAttack) || !(handle.Action is UnitAnimationActionCover)))
				{
					m_CurrentAction.Release();
				}
				m_CurrentAction = handle;
				m_CurrentAction.StartInternal();
				foreach (AnimationActionHandle sequencedAction in m_SequencedActions)
				{
					if (sequencedAction.Action != null)
					{
						PFLog.Default.Log("Cleared sequnced action: {0}", sequencedAction.Action.name);
					}
					else
					{
						PFLog.Default.Log("Cleared sequnced action: (destroyed)");
					}
					sequencedAction.MarkInterrupted();
				}
				m_SequencedActions.Clear();
			}
			else if (m_CurrentAction == null || m_CurrentAction.DontReleaseOnInterrupt)
			{
				AddActionHandle(handle);
				m_CurrentAction = handle;
				m_CurrentAction.StartInternal();
			}
			else if (m_SequencedActions.Count > 10)
			{
				PFLog.Default.Warning($"Animation manager {this} has too many SequencedActions! This might be a leak.");
			}
			else
			{
				m_SequencedActions.Enqueue(handle);
			}
		}
	}

	public AnimationActionHandle Execute(AnimationActionBase action)
	{
		AnimationActionHandle animationActionHandle = CreateHandle(action);
		Execute(animationActionHandle);
		return animationActionHandle;
	}

	public void AddAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask, ClipDurationType durType = ClipDurationType.Default)
	{
		if (handle != m_CurrentAction && !handle.DontReleaseOnInterrupt && !handle.IsAdditive)
		{
			PFLog.Default.Error($"Can't start animation on interrupted action {handle.Action}");
			return;
		}
		AnimationBase activeAnimation = PlayAnimationClip(handle, clipWrapper, avatarMask, durType);
		handle.ActiveAnimation = activeAnimation;
		handle.ActiveAnimation.TransitionIn = GetTransitionInDuration(handle.ActiveAnimation);
		handle.ActiveAnimation.TransitionOut = GetTransitionOutDuration(handle);
		float num = 0f;
		switch (durType)
		{
		case ClipDurationType.Endless:
			num = 0f;
			break;
		case ClipDurationType.Oneshot:
			num = clipWrapper.Length;
			break;
		case ClipDurationType.Default:
			num = handle.ActiveAnimation.GetDuration();
			break;
		}
		bool flag = num > 0f;
		handle.ActiveAnimation.TransitionOutStartTime = (flag ? Mathf.Max(num - handle.ActiveAnimation.TransitionOut, 0.001f) : 0f);
		m_ActiveAnimations.Add(handle.ActiveAnimation);
	}

	private PlayableInfo PlayAnimationClip(AnimationActionHandle handle, AnimationClipWrapper clipWrapper, AvatarMask avatarMask, ClipDurationType duration)
	{
		if (clipWrapper == null)
		{
			throw new Exception("Animation clip wrapper is null.");
		}
		if (clipWrapper.AnimationClip == null)
		{
			throw new Exception("Animation clip wrapper has a null animation clip.");
		}
		AnimancerLayer animancerLayer = m_AnimancerLayers[(int)handle.AnimationLayer];
		if (animancerLayer.Mask != avatarMask)
		{
			animancerLayer.Mask = avatarMask;
		}
		AnimancerState orCreateState = animancerLayer.GetOrCreateState(clipWrapper.AnimationClip);
		orCreateState.Time = 0f;
		AnimancerState animancerState = animancerLayer.Play(orCreateState, GetTransitionInDuration(handle.Action, clipWrapper.AnimationClip));
		animancerState.Speed = handle.SpeedScale;
		PlayableInfo result = new PlayableInfo(handle, animancerState);
		SetupAnimationEvents(clipWrapper, duration, animancerState);
		return result;
	}

	private void SetupAnimationEvents(AnimationClipWrapper clipWrapper, ClipDurationType duration, AnimancerState animancerState)
	{
		if (!animancerState.Events(this, out var events))
		{
			return;
		}
		float length = clipWrapper.Length;
		AnimationClipEvent[] eventsSorted = clipWrapper.EventsSorted;
		foreach (AnimationClipEvent @event in eventsSorted)
		{
			float normalizedTime = @event.Time / length;
			events.Add(new AnimancerEvent(normalizedTime, delegate
			{
				if (duration != ClipDurationType.Oneshot || !(AnimancerEvent.Current.State.NormalizedTime > 1f))
				{
					@event.Start(this);
				}
			}));
		}
	}

	public void UpdateLocomotionMixerAnimations([NotNull] LocomotionMixerAnimations animations)
	{
		m_LocomotionMixerState.DestroyChildren();
		foreach (var item in animations.Where(((AnimationClipWrapper ClipWrapper, Vector2 Threshold) entry) => entry.ClipWrapper.Or(null)?.AnimationClip != null))
		{
			m_LocomotionMixerState.Add(item.ClipWrapper.AnimationClip, item.Threshold);
		}
		if (animations.ClipWithFootstepEvents != null)
		{
			SetupAnimationEvents(animations.ClipWithFootstepEvents, ClipDurationType.Endless, m_LocomotionMixerState);
		}
		m_LocomotionMixerState.DontSynchronize(m_LocomotionMixerState.GetChild(0));
	}

	public void UpdateLocomotionParameters(Vector2 faceDirection, Vector2 moveDirection, float speed, float maxSpeed)
	{
		Vector2 normalized = faceDirection.normalized;
		Vector2 normalized2 = moveDirection.normalized;
		float x = Vector2.Dot(normalized, normalized2);
		float y = Vector2.Dot(Vector2.Perpendicular(normalized), normalized2);
		float num = ((maxSpeed > 0f) ? (speed / maxSpeed) : 0f);
		m_LocomotionMixerState.Parameter = new Vector2(x, y) * num;
	}

	public void PlayLocomotionMixer(float fadeDuration = 0.2f)
	{
		m_LocomotionLayer.Play(m_LocomotionMixerState, fadeDuration);
	}

	private int GetActiveTransformCount(AvatarMask avatarMask)
	{
		if (avatarMask == null)
		{
			return int.MaxValue;
		}
		int transformCount = avatarMask.transformCount;
		int num = 0;
		for (int i = 0; i < transformCount; i++)
		{
			if (avatarMask.GetTransformActive(i))
			{
				num++;
			}
		}
		return num;
	}

	internal Transition FindTransition(PlayableInfo fromPlayableInfo, PlayableInfo toPlayableInfo, List<Transition> transitions)
	{
		AnimationClip activeClip = fromPlayableInfo.GetActiveClip();
		AnimationClip activeClip2 = toPlayableInfo.GetActiveClip();
		Transition transition = null;
		Transition result = null;
		foreach (Transition transition2 in transitions)
		{
			if (transition2.FromAction == fromPlayableInfo.Handle.Action && transition2.ToAction == toPlayableInfo.Handle.Action && (transition2.FromClip == activeClip || transition2.FromClip == null) && (transition2.ToClip == activeClip2 || transition2.ToClip == null))
			{
				result = transition2;
				if (transition2.FromClip == activeClip && transition2.ToClip == activeClip2)
				{
					transition = transition2;
				}
			}
		}
		if (transition != null)
		{
			return transition;
		}
		return result;
	}

	internal float GetTransitionInDuration(AnimationBase animation)
	{
		float result = animation.Handle.Action.TransitionIn;
		if (m_CurrentAction?.ActiveAnimation != null && m_CurrentAction != animation.Handle && m_ToTransitions.TryGetValue(animation.Handle.Action, out var value))
		{
			AnimationClip activeClip = m_CurrentAction.ActiveAnimation.GetActiveClip();
			AnimationClip activeClip2 = animation.GetActiveClip();
			foreach (Transition item in value)
			{
				if (item.FromAction == m_CurrentAction.Action)
				{
					if (activeClip != null && item.FromClip == activeClip)
					{
						_ = item.ToClip == activeClip2;
						result = item.Duration;
					}
					else
					{
						result = item.Duration;
					}
					break;
				}
			}
		}
		return result;
	}

	internal float GetTransitionOutDuration(AnimationActionHandle actionHandle)
	{
		float num = actionHandle.Action.TransitionOut;
		if (m_FromTransitions.TryGetValue(actionHandle.Action, out var value))
		{
			foreach (Transition item in value)
			{
				if (item.Duration > num)
				{
					num = item.Duration;
				}
			}
		}
		return num;
	}

	internal float GetTransitionInDuration(AnimationActionBase action, AnimationClip animationClip)
	{
		float result = action.TransitionIn;
		if (m_ToTransitions.TryGetValue(action, out var value))
		{
			AnimationClip animationClip2 = m_CurrentAction?.ActiveAnimation?.GetActiveClip();
			foreach (Transition item in value)
			{
				if (item.FromAction == m_CurrentAction.Action)
				{
					if (animationClip2 != null && item.FromClip == animationClip2)
					{
						_ = item.ToClip == animationClip;
						result = item.Duration;
					}
					else
					{
						result = item.Duration;
					}
					break;
				}
			}
		}
		return result;
	}

	public string DebugGetHierarchyPath()
	{
		return base.transform.GetHierarchyPath();
	}
}
