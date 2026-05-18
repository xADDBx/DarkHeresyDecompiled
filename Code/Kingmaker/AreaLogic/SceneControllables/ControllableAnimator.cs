using System;
using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.AreaLogic.SceneControllables;

[RequireComponent(typeof(Animator))]
public class ControllableAnimator : ControllableComponent, IUpdatable, IInterpolatable
{
	[Serializable]
	public class ActionsOnAnimationEvent
	{
		public int EventId;

		[SerializeField]
		public ActionList Actions = new ActionList();
	}

	[Serializable]
	public class ActionHoldersOnAnimationEvent
	{
		public int EventId;

		[SerializeField]
		public List<ActionsReference> ActionHolders = new List<ActionsReference>();
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("ControllableComponent");

	private const int SubTickCount = 16;

	[SerializeField]
	private bool m_AnimateOutsideOfCameraView;

	private Animator m_Animator;

	private PlayableGraph m_Graph;

	private AnimatorControllerPlayable m_Playable;

	private static readonly int State = Animator.StringToHash("State");

	[ShowIf("m_ObsoleteEventsExist")]
	[Obsolete]
	public List<ActionsOnAnimationEvent> ActionsOnEvent = new List<ActionsOnAnimationEvent>();

	[Obsolete]
	private Dictionary<int, ActionList> m_ActionsMapObsolete = new Dictionary<int, ActionList>();

	public List<ActionHoldersOnAnimationEvent> ActionHoldersOnEvent = new List<ActionHoldersOnAnimationEvent>();

	private Dictionary<int, List<ActionsReference>> m_ActionHolders = new Dictionary<int, List<ActionsReference>>();

	private float m_SubTickDeltaTime;

	private int m_SubTicksRemains;

	private readonly List<int> m_EventsDuringSubTicks = new List<int>();

	private readonly List<ControllableState> m_StatesRequestQueue = new List<ControllableState>();

	private bool m_ObsoleteEventsExist => ActionsOnEvent.Count > 0;

	protected override void Awake()
	{
		base.Awake();
		InitAnimator();
		foreach (ActionsOnAnimationEvent item in ActionsOnEvent)
		{
			m_ActionsMapObsolete[item.EventId] = item.Actions;
		}
		foreach (ActionHoldersOnAnimationEvent item2 in ActionHoldersOnEvent)
		{
			if (m_ActionHolders.TryGetValue(item2.EventId, out var value))
			{
				value.AddRange(item2.ActionHolders);
			}
			else
			{
				m_ActionHolders[item2.EventId] = item2.ActionHolders;
			}
		}
	}

	protected override void OnEnable()
	{
		try
		{
			InitAnimator();
			EnsureSubscribed();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		try
		{
			UnsubscribeAndReset();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		base.OnDisable();
	}

	private void InitAnimator()
	{
		if (!(m_Animator != null))
		{
			m_EventsDuringSubTicks.Clear();
			m_StatesRequestQueue.Clear();
			m_Animator = GetComponent<Animator>();
			m_Animator.enabled = true;
			m_Animator.speed = 0f;
			m_Animator.cullingMode = ((!m_AnimateOutsideOfCameraView) ? AnimatorCullingMode.CullCompletely : AnimatorCullingMode.AlwaysAnimate);
			m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			m_Graph = PlayableGraph.Create();
			m_Playable = AnimatorControllerPlayable.Create(m_Graph, m_Animator.runtimeAnimatorController);
			AnimationPlayableOutput.Create(m_Graph, "Output", m_Animator).SetSourcePlayable(m_Playable);
			m_Graph.Play();
		}
	}

	private void EnsureSubscribed()
	{
		Game.Instance.Controllers?.CustomUpdateBeforePhysicsController.AddUnique(this);
		Game.Instance.Controllers?.InterpolationController.AddUnique(this);
	}

	private void UnsubscribeAndReset()
	{
		Game.Instance.Controllers?.CustomUpdateBeforePhysicsController.Remove(this);
		Game.Instance.Controllers?.InterpolationController.Remove(this);
		m_EventsDuringSubTicks.Clear();
		m_StatesRequestQueue.Clear();
		m_SubTickDeltaTime = 0f;
		m_SubTicksRemains = 0;
	}

	public override void SetState(ControllableState state)
	{
		base.SetState(state);
		InitAnimator();
		EnsureSubscribed();
		m_StatesRequestQueue.Add(state);
	}

	void IUpdatable.Tick(float delta)
	{
		using (ProfileScope.New("ControllableAnimator Sim Tick"))
		{
			for (int i = 0; i < m_SubTicksRemains; i++)
			{
				m_Graph.Evaluate(m_SubTickDeltaTime);
			}
			foreach (int eventsDuringSubTick in m_EventsDuringSubTicks)
			{
				if (m_ActionsMapObsolete.TryGetValue(eventsDuringSubTick, out var value))
				{
					try
					{
						value.Run();
					}
					catch (Exception ex)
					{
						Logger.Exception(ex);
					}
				}
				if (!m_ActionHolders.TryGetValue(eventsDuringSubTick, out var value2))
				{
					continue;
				}
				try
				{
					foreach (ActionsReference item in value2)
					{
						item.Get().Run();
					}
				}
				catch (Exception ex2)
				{
					Logger.Exception(ex2);
				}
			}
			m_EventsDuringSubTicks.Clear();
			foreach (ControllableState item2 in m_StatesRequestQueue)
			{
				if (item2.State.HasValue)
				{
					m_Playable.SetInteger(State, item2.State.Value);
				}
				if (item2.Active.HasValue)
				{
					base.gameObject.SetActive(item2.Active.Value);
				}
			}
			m_StatesRequestQueue.Clear();
			if (m_Animator == null || m_Animator.gameObject == null || !m_Animator.gameObject.activeInHierarchy)
			{
				UnsubscribeAndReset();
				return;
			}
			m_SubTickDeltaTime = delta / 16f;
			m_SubTicksRemains = 16;
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		using (ProfileScope.New("ControllableAnimator Int Tick"))
		{
			int b = Mathf.RoundToInt(16f * progress);
			b = Mathf.Min(m_SubTicksRemains, b);
			if (b != 0)
			{
				m_SubTicksRemains -= b;
				for (int i = 0; i < b; i++)
				{
					m_Graph.Evaluate(m_SubTickDeltaTime);
				}
			}
		}
	}

	public void RunActions(int eventId)
	{
		if (m_ActionsMapObsolete.ContainsKey(eventId))
		{
			m_EventsDuringSubTicks.Add(eventId);
		}
		if (m_ActionHolders.ContainsKey(eventId))
		{
			m_EventsDuringSubTicks.Add(eventId);
		}
	}
}
