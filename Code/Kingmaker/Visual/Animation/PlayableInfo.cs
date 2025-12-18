using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

public class PlayableInfo : AnimationBase
{
	private AnimancerState m_AnimancerState;

	private AnimationClipEvent[] m_Events;

	private Dictionary<AnimationClipEvent, Action> m_LoopedEventToStopAction = new Dictionary<AnimationClipEvent, Action>();

	private int m_NextEventIndex;

	private float m_WeightMultiplier = 1f;

	private float m_Weight;

	private int m_LoopCount;

	private bool m_IsFirstUpdate;

	private bool m_IsSuspended;

	private AnimationClip m_Clip;

	private bool m_IsStopped;

	public bool IsSuspended => m_IsSuspended;

	public bool IsStopped => m_IsStopped;

	internal PlayableInfo(AnimationActionHandle handle, AnimancerState animancerState)
		: base(handle)
	{
		m_AnimancerState = animancerState;
		Reset(handle);
	}

	internal void Reset(AnimationActionHandle handle)
	{
		StopEvents();
		base.Handle = handle;
		base.CreationTime = ((handle.Manager.UpdateMode == DirectorUpdateMode.UnscaledGameTime) ? Game.Instance.Controllers.TimeController.RealTime : Game.Instance.Controllers.TimeController.GameTime);
		SetSpeed(handle.SpeedScale);
		SetTime(0f);
		base.State = AnimationState.TransitioningIn;
		m_NextEventIndex = 0;
		m_WeightMultiplier = 1f;
		m_Weight = 0f;
		m_LoopCount = 0;
		m_IsFirstUpdate = true;
		m_IsSuspended = false;
		m_IsStopped = false;
	}

	public override void StopEvents(IEnumerable<AnimationClipEvent> events)
	{
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			AnimationClipEvent @event = item.Key;
			Action value = item.Value;
			if (events.FirstOrDefault((AnimationClipEvent _event) => _event.DoNotStartIfStarted(@event)) == null)
			{
				value();
			}
		}
		m_LoopedEventToStopAction.Clear();
		m_IsStopped = true;
	}

	public override void StopEvents()
	{
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			item.Value?.Invoke();
		}
		m_LoopedEventToStopAction.Clear();
		m_IsStopped = true;
	}

	public void SuspendEvents()
	{
		if (m_IsSuspended)
		{
			return;
		}
		foreach (AnimationClipEvent item in m_LoopedEventToStopAction.Keys.ToTempList())
		{
			m_LoopedEventToStopAction[item]?.Invoke();
			m_LoopedEventToStopAction[item] = null;
		}
		m_IsSuspended = true;
	}

	public void ResumeEvents()
	{
		if (!m_IsSuspended)
		{
			return;
		}
		foreach (AnimationClipEvent item in m_LoopedEventToStopAction.Keys.ToTempList())
		{
			m_LoopedEventToStopAction[item] = item.Start(base.Handle.Manager as AnimationManager);
		}
		m_IsSuspended = false;
	}

	public void StartEvents(float time)
	{
		m_IsStopped = false;
		SkipEvents(time);
	}

	public void StartEvents(IEnumerable<AnimationClipEvent> events, float time)
	{
		Dictionary<AnimationClipEvent, Action> dictionary = null;
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			bool flag = false;
			foreach (AnimationClipEvent @event in events)
			{
				if (@event.DoNotStartIfStarted(item.Key) && item.Value != null)
				{
					dictionary = dictionary ?? new Dictionary<AnimationClipEvent, Action>();
					dictionary.Add(@event, item.Value);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				item.Value?.Invoke();
			}
		}
		if (dictionary == null)
		{
			m_LoopedEventToStopAction.Clear();
		}
		else
		{
			m_LoopedEventToStopAction = dictionary;
		}
		m_Events = events?.ToArray();
		if (m_Events != null)
		{
			Array.Sort(m_Events, (AnimationClipEvent evt1, AnimationClipEvent evt2) => evt1.Time.CompareTo(evt2.Time));
		}
		SetTime(time);
		m_NextEventIndex = 0;
		m_LoopCount = 0;
		m_IsFirstUpdate = true;
		m_IsStopped = false;
		while (m_NextEventIndex < m_Events.Length && m_Events[m_NextEventIndex].Time < time)
		{
			m_NextEventIndex++;
		}
	}

	public override void UpdateEvents()
	{
	}

	public void SkipEvents(float time)
	{
		if (time < m_LastSetTime)
		{
			m_NextEventIndex = 0;
			m_LoopCount++;
		}
		SetTime(time);
		while (m_NextEventIndex < m_Events.Length && m_Events[m_NextEventIndex].Time < time)
		{
			m_NextEventIndex++;
		}
	}

	public sealed override AnimationClip GetPlayableClip()
	{
		return m_AnimancerState.Clip;
	}

	public override RuntimeAnimatorController GetPlayableController()
	{
		return null;
	}

	public override AnimationClip GetActiveClip()
	{
		return m_AnimancerState.Clip;
	}

	public override void SetWeight(float weight)
	{
	}

	public override void SetWeightMultiplier(float weight)
	{
		float weight2 = GetWeight();
		m_WeightMultiplier = weight;
		SetWeight(weight2);
	}

	public override float GetWeight()
	{
		return m_AnimancerState.Weight;
	}

	public float GetAdjustedWeight()
	{
		return m_Weight * m_WeightMultiplier;
	}

	public void SetAdjustedWeight(float weight)
	{
		if (!(m_WeightMultiplier <= 0.001f))
		{
			m_Weight = weight / m_WeightMultiplier;
		}
	}

	public override void SetSpeed(float speed)
	{
		if (NeedChangeSpeed(speed))
		{
			m_LastSetSpeed = speed;
			m_AnimancerState.Speed = speed;
		}
	}

	public override float GetSpeed()
	{
		return m_LastSetSpeed;
	}

	public override float GetTime()
	{
		return base.Time;
	}

	public override void SetTime(float time)
	{
		if (NeedChangeTime(time))
		{
			m_LastSetTime = time;
			base.Time = time;
			m_AnimancerState.Time = time;
		}
	}

	public override void RemoveFromManager()
	{
	}

	public override AnimationBase Find(AvatarMask mask, bool isAdditive)
	{
		return null;
	}
}
