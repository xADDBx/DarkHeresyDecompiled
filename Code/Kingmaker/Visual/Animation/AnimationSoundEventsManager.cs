using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Animation.Events;

namespace Kingmaker.Visual.Animation;

public class AnimationSoundEventsManager
{
	private enum LoopedSoundEventState
	{
		Playing,
		Suspended,
		Stopped
	}

	private class LoopedEventData
	{
		public readonly AnimancerState Animation;

		public readonly Action StopAction;

		public LoopedSoundEventState State;

		public bool IsPlaying => State == LoopedSoundEventState.Playing;

		public bool IsSuspended => State == LoopedSoundEventState.Suspended;

		public bool IsStoppped => State == LoopedSoundEventState.Stopped;

		public LoopedEventData(AnimancerState animation, Action stopAction)
		{
			Animation = animation;
			StopAction = stopAction;
			State = LoopedSoundEventState.Playing;
		}
	}

	private readonly AnimationManager m_AnimationManager;

	private readonly MechanicEntityView m_View;

	private Dictionary<AnimationClipEventSound, LoopedEventData> m_LoopedEvents = new Dictionary<AnimationClipEventSound, LoopedEventData>();

	private List<AnimationClipEventSound> m_StoppedEvents = new List<AnimationClipEventSound>();

	public AnimationSoundEventsManager(AnimationManager animationManager)
	{
		m_AnimationManager = animationManager;
		m_View = animationManager.GetComponentInParent<MechanicEntityView>();
	}

	public void Update()
	{
		m_StoppedEvents.Clear();
		foreach (var (@event, loopedEventData2) in m_LoopedEvents)
		{
			if (loopedEventData2.IsPlaying)
			{
				if (!loopedEventData2.Animation.IsPlaying)
				{
					StopLoopedSound(@event, loopedEventData2);
				}
				else if (!IsAllowed(@event))
				{
					SuspendLoopedSound(@event, loopedEventData2);
				}
			}
			else if (loopedEventData2.IsSuspended)
			{
				if (!loopedEventData2.Animation.IsPlaying)
				{
					StopLoopedSound(@event, loopedEventData2);
				}
				else if (IsAllowed(@event))
				{
					ResumeLoopedSound(@event);
				}
			}
		}
		foreach (AnimationClipEventSound stoppedEvent in m_StoppedEvents)
		{
			m_LoopedEvents.Remove(stoppedEvent);
		}
	}

	public void StopAllLoopedSounds()
	{
		foreach (var (@event, data) in m_LoopedEvents)
		{
			StopLoopedSound(@event, data);
		}
		m_LoopedEvents.Clear();
	}

	public void PostSoundEvent(AnimationClipEventSound @event)
	{
		if (CanStartSound(@event))
		{
			PostSoundEventInternal(@event);
		}
	}

	private bool CanStartSound(AnimationClipEventSound @event)
	{
		if (IsAllowed(@event))
		{
			return m_LoopedEvents.Keys.All((AnimationClipEventSound x) => x.Name != @event.Name);
		}
		return false;
	}

	private bool IsAllowed(AnimationClipEventSound @event)
	{
		return @event.AnimancerState.LayerIndex >= m_AnimationManager.HighestActiveLayerIndex;
	}

	private void PostSoundEventInternal(AnimationClipEventSound @event)
	{
		Action action = @event.PostSoundEvent(m_View);
		if (action != null)
		{
			m_LoopedEvents[@event] = new LoopedEventData(@event.AnimancerState, action);
		}
	}

	private void StopLoopedSound(AnimationClipEventSound @event, LoopedEventData data)
	{
		m_StoppedEvents.Add(@event);
		data.StopAction?.Invoke();
		data.State = LoopedSoundEventState.Stopped;
	}

	private void SuspendLoopedSound(AnimationClipEventSound @event, LoopedEventData data)
	{
		data.StopAction?.Invoke();
		data.State = LoopedSoundEventState.Suspended;
	}

	private void ResumeLoopedSound(AnimationClipEventSound @event)
	{
		PostSoundEventInternal(@event);
	}
}
