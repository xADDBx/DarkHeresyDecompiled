using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Animancer.TransitionLibraries;
using JetBrains.Annotations;
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
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(AnimancerComponent))]
public class AnimationManager : MonoBehaviour, IAnimationManager
{
	private bool m_IsInitialized;

	protected AnimancerComponent m_Animancer;

	private float m_DefaultSpeed = 1f;

	private AnimationActionHandle m_CurrentAction;

	private AnimationClip m_CurrentAnimationClip;

	private AnimationClip m_NextAnimationClip;

	private readonly List<AnimationActionHandle> m_ActiveActions = new List<AnimationActionHandle>();

	private readonly Queue<AnimationActionHandle> m_SequencedActions = new Queue<AnimationActionHandle>();

	private readonly List<AnimationBase> m_ActiveAnimations = new List<AnimationBase>();

	private CountableFlag m_RotationForbidden = new CountableFlag();

	private readonly CountingGuard m_Disabled = new CountingGuard();

	public virtual StatefulRandom StatefulRandom => PFStatefulRandom.Visuals.Animation3;

	public TransitionLibrary TransitionLibrary => m_Animancer.Transitions?.Library;

	public int HighestActiveLayerIndex { get; private set; }

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

	public AnimationSoundEventsManager SoundEventsManager { get; private set; }

	public GameObject GameObject => base.gameObject;

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
			bool value2 = m_Disabled.Value;
			m_Disabled.Value = value;
			m_Animancer.Graph.Speed = DefaultSpeed;
			if (!value2 && (bool)m_Disabled)
			{
				SoundEventsManager.StopAllLoopedSounds();
			}
		}
	}

	public Animator Animator => m_Animancer.Animator;

	public IReadOnlyList<AnimationActionHandle> ActiveActions => m_ActiveActions;

	public List<AnimationBase> ActiveAnimations => m_ActiveAnimations;

	public Queue<AnimationActionHandle> SequencedActions => m_SequencedActions;

	public AnimationActionHandle CurrentAction => m_CurrentAction;

	protected void OnEnable()
	{
		Game.Instance.Controllers.AnimationManagerController.Subscribe(this);
		Initialize();
		OnAfterEnabled();
	}

	private void Initialize()
	{
		if (!m_IsInitialized)
		{
			SoundEventsManager = new AnimationSoundEventsManager(this);
			m_Animancer = GetComponent<AnimancerComponent>();
			m_Animancer.ActionOnDisable = AnimancerComponent.DisableAction.Pause;
			OnInitialize();
			m_IsInitialized = true;
		}
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnAfterEnabled()
	{
	}

	protected virtual void OnBeforeDisabled()
	{
	}

	protected void OnDisable()
	{
		OnBeforeDisabled();
		Game.Instance.Controllers.AnimationManagerController.Unsubscribe(this);
	}

	public void CustomUpdate(float dt)
	{
		using (Counters.AnimationManager?.Measure())
		{
			if (Disabled)
			{
				return;
			}
			using (ProfileScope.New("AnimationManager.UpdateAnimations"))
			{
				UpdateAnimations(dt);
			}
			using (ProfileScope.New("AnimationManager.UpdateActions"))
			{
				UpdateActions(dt);
			}
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
		UpdateCurrentAnimationClip();
		SoundEventsManager.Update();
	}

	private void StartFadeOutAnimation(AnimationBase animation)
	{
		if (m_ActiveAnimations.Any((AnimationBase a) => a.GetActiveClip() == animation.GetActiveClip() && a.Handle.AnimationLayer == animation.Handle.AnimationLayer))
		{
			return;
		}
		AnimancerLayer animancerLayer = m_Animancer.Layers[(int)animation.Handle.AnimationLayer];
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

	private void UpdateCurrentAnimationClip()
	{
		AnimationClip animationClip = null;
		AnimationClip animationClip2 = null;
		HighestActiveLayerIndex = 0;
		int count = m_Animancer.Layers.Count;
		for (int i = 0; i < count; i++)
		{
			AnimancerLayer animancerLayer = m_Animancer.Layers[i];
			if (!(animancerLayer.Weight < 0.5f) && !animancerLayer.IsAdditive)
			{
				HighestActiveLayerIndex = i;
				animationClip = animationClip2;
				animationClip2 = GetMostWeightedClip(animancerLayer.CurrentState);
			}
		}
		if (!(animationClip2 == m_CurrentAnimationClip) || !(animationClip == m_NextAnimationClip))
		{
			m_CurrentAnimationClip = animationClip2;
			m_NextAnimationClip = animationClip;
			if (TryGetTransitionBetween(m_CurrentAnimationClip, m_NextAnimationClip, out var transitionTime))
			{
				m_CurrentAction.ActiveAnimation.ChangeTransitionTime(transitionTime);
			}
		}
	}

	private AnimationClip GetMostWeightedClip(AnimancerState state)
	{
		if (state.Clip != null)
		{
			return state.Clip;
		}
		if (!(state is ManualMixerState manualMixerState))
		{
			return null;
		}
		float num = 0f;
		AnimationClip result = null;
		for (int i = 0; i < manualMixerState.ChildCount; i++)
		{
			AnimancerState child = manualMixerState.GetChild(i);
			if (child.Weight > num)
			{
				result = GetMostWeightedClip(child);
				num = child.Weight;
			}
		}
		if (!(num > 0f))
		{
			return null;
		}
		return result;
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

	protected void ResetTransitionsAndActions()
	{
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

	public bool TryExecute([NotNull] AnimationActionBase animationAction)
	{
		AnimationActionHandle handle;
		return TryExecute(animationAction, null, out handle);
	}

	public bool TryExecute([NotNull] AnimationActionBase animationAction, out AnimationActionHandle handle)
	{
		return TryExecute(animationAction, null, out handle);
	}

	public virtual bool TryExecute([NotNull] AnimationActionBase animationAction, Action<AnimationActionHandle> initializer, out AnimationActionHandle handle)
	{
		handle = CreateHandle(animationAction);
		if (handle != null)
		{
			initializer?.Invoke(handle);
			Execute(handle);
		}
		return handle != null;
	}

	protected virtual AnimationActionHandle CreateHandle([NotNull] AnimationActionBase animationAction)
	{
		return new AnimationActionHandle(animationAction, this);
	}

	protected virtual void Execute(AnimationActionHandle handle)
	{
		if (handle == null)
		{
			PFLog.Animations.Error("AnimationActionHandle is null");
			return;
		}
		using (ProfileScope.New("Animator.Execute " + handle.Action.name))
		{
			if (this != (AnimationManager)handle.Manager)
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
		if (handle != m_CurrentAction && !handle.DontReleaseOnInterrupt)
		{
			PFLog.Default.Error($"Can't start animation on interrupted action {handle.Action}");
			return;
		}
		handle.ActiveAnimation = PlayAnimationClip(handle, clipWrapper, avatarMask, durType);
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
		float transitionInDuration = GetTransitionInDuration(handle.Action, clipWrapper.AnimationClip);
		AnimancerState animancerState = PlayAnimationClip(clipWrapper, (int)handle.AnimationLayer, handle.SpeedScale, transitionInDuration, avatarMask, duration);
		return new PlayableInfo(handle, animancerState, clipWrapper.MechanicEventsSorted)
		{
			TransitionIn = transitionInDuration,
			TransitionOut = handle.Action.TransitionOut
		};
	}

	protected AnimancerState PlayAnimationClip(AnimationClipWrapper clipWrapper, int animancerLayerIndex = 0, float speedScale = 1f, float fadeInTime = 0.2f, AvatarMask avatarMask = null, ClipDurationType duration = ClipDurationType.Default, Action onEnd = null)
	{
		if (clipWrapper == null)
		{
			throw new Exception("Animation clip wrapper is null.");
		}
		if (clipWrapper.AnimationClip == null)
		{
			throw new Exception("Animation clip wrapper has a null animation clip.");
		}
		AnimancerLayer animancerLayer = m_Animancer.Layers[animancerLayerIndex];
		if (animancerLayer.Mask != avatarMask)
		{
			animancerLayer.Mask = avatarMask;
		}
		AnimancerState orCreateState = animancerLayer.GetOrCreateState(clipWrapper.AnimationClip);
		orCreateState.Time = 0f;
		AnimancerState animancerState = animancerLayer.Play(orCreateState, fadeInTime);
		animancerState.Speed = speedScale;
		Action onEnd2 = ((!clipWrapper.IsLooping && duration == ClipDurationType.Endless) ? ((Action)delegate
		{
			onEnd?.Invoke();
			animancerState.Time = 0f;
		}) : onEnd);
		SetupAnimationEvents(clipWrapper, duration, animancerState, onEnd2);
		return animancerState;
	}

	protected void SetupAnimationEvents(AnimationClipWrapper clipWrapper, ClipDurationType duration, AnimancerState animancerState, Action onEnd = null)
	{
		AnimancerEvent.Sequence events;
		bool num = animancerState.Events(this, out events);
		events.OnEnd = onEnd;
		if (!num)
		{
			return;
		}
		float length = clipWrapper.Length;
		AnimationClipEvent[] animationEventsSorted = clipWrapper.AnimationEventsSorted;
		foreach (AnimationClipEvent @event in animationEventsSorted)
		{
			@event.UserData = animancerState;
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

	private float GetTransitionOutDuration(AnimationActionHandle handle)
	{
		return handle.ActiveAnimation.TransitionOut;
	}

	private float GetTransitionInDuration(AnimationActionBase action, AnimationClip animationClip)
	{
		if (TryGetTransitionBetween(m_CurrentAnimationClip, animationClip, out var transitionTime))
		{
			return transitionTime;
		}
		return action.TransitionIn;
	}

	private bool TryGetTransitionBetween(AnimationClip from, AnimationClip to, out float transitionTime)
	{
		transitionTime = 0f;
		if (from == null || to == null || TransitionLibrary == null)
		{
			return false;
		}
		if (TransitionLibrary.TryGetTransition(StringReference.Get(from.name), out var transition) && TransitionLibrary.TryGetTransition(StringReference.Get(to.name), out var transition2))
		{
			TransitionAsset transitionAsset = transition.Transition as TransitionAsset;
			transitionTime = transition2.GetFadeDuration(transitionAsset.Transition);
			return true;
		}
		return false;
	}

	public void Rebind()
	{
		Animator.Rebind();
		m_Animancer.Graph.Evaluate();
	}

	public string DebugGetHierarchyPath()
	{
		return base.transform.GetHierarchyPath();
	}
}
