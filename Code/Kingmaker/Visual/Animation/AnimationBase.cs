using System;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

public abstract class AnimationBase
{
	private float m_PreviousTime;

	private float m_NextTime;

	protected float m_LastSetSpeed;

	protected float m_LastSetTime;

	private AnimationClipEvent[] m_MechanicEvents;

	private int m_NextMechanicEventIndex;

	protected float Time
	{
		get
		{
			return m_PreviousTime;
		}
		set
		{
			m_PreviousTime = value;
			m_NextTime = value;
		}
	}

	public AnimationActionHandle Handle { get; protected set; }

	public TimeSpan CreationTime { get; protected set; }

	public AnimationState State { get; protected set; }

	public bool DoNotZeroOtherAnimations { get; set; }

	public float TransitionIn { get; internal set; }

	public float TransitionOut { get; internal set; }

	public float TransitionOutStartTime { get; internal set; }

	public float WeightFromManager { get; private set; }

	protected bool NeedChangeSpeed(float speed)
	{
		if (Mathf.Approximately(speed, 0f) || Mathf.Approximately(m_LastSetSpeed, 0f) || Mathf.Approximately(speed, 1f) || Mathf.Approximately(m_LastSetSpeed, 1f))
		{
			return true;
		}
		return Mathf.Abs(speed - m_LastSetSpeed) >= 0.001f;
	}

	protected bool NeedChangeTime(float time)
	{
		if (m_LastSetTime == time)
		{
			return false;
		}
		if (time == 0f || m_LastSetTime == 0f || time == 1f || m_LastSetTime == 1f)
		{
			return true;
		}
		return Mathf.Abs(time - m_LastSetTime) >= 0.001f;
	}

	protected AnimationBase(AnimationActionHandle handle)
	{
		Handle = handle;
	}

	protected void InitOrIncrementTime(float deltaTime)
	{
		float num = deltaTime * GetSpeed();
		if (!TryInitInterpolationTime(num) || Handle.SkipFirstTick)
		{
			m_PreviousTime = m_NextTime;
			m_NextTime += num;
		}
	}

	public abstract AnimationClip GetPlayableClip();

	public abstract AnimationClip GetActiveClip();

	public abstract float GetWeight();

	public abstract void SetSpeed(float speed);

	public abstract float GetSpeed();

	public abstract float GetTime();

	public abstract void SetTime(float time);

	protected abstract void FireMechanicEvent(AnimationClipEvent e);

	public abstract void RemoveFromManager();

	public float GetDuration()
	{
		AnimationClip playableClip = GetPlayableClip();
		if (playableClip != null && !playableClip.isLooping)
		{
			return playableClip.length;
		}
		return 0f;
	}

	[CanBeNull]
	public abstract AnimationBase Find([CanBeNull] AvatarMask mask, bool isAdditive);

	internal void StartTransitionOut()
	{
		if (State != AnimationState.TransitioningOut && State != AnimationState.Finished)
		{
			TransitionOutStartTime = Time;
			State = AnimationState.TransitioningOut;
			if (this == Handle.ActiveAnimation)
			{
				Handle.Action.OnTransitionOutStarted(Handle);
			}
		}
	}

	public void ChangeTransitionTime(float time)
	{
		if (State != AnimationState.TransitioningOut && State != AnimationState.Finished)
		{
			TransitionOutStartTime = Mathf.Max(Time, TransitionOutStartTime + TransitionOut - time);
			TransitionOut = time;
		}
	}

	internal void Update(float deltaTime, float? weightFromManager = null)
	{
		if (IsJustCreated())
		{
			return;
		}
		using (ProfileScope.New("Animation.UpdateInternal"))
		{
			InitOrIncrementTime(deltaTime);
			TryFireMechanicEvent();
			switch (State)
			{
			case AnimationState.TransitioningIn:
				using (ProfileScope.New("TransitioningIn"))
				{
					if (TransitionIn <= 0f || Time >= TransitionIn)
					{
						if (TransitionOutStartTime > 0f && Time >= TransitionOutStartTime)
						{
							StartTransitionOut();
						}
						else
						{
							State = AnimationState.Playing;
						}
					}
					break;
				}
			case AnimationState.Playing:
				using (ProfileScope.New("Playing"))
				{
					if ((TransitionOutStartTime > 0f && Time >= TransitionOutStartTime) || Handle.IsReleased)
					{
						StartTransitionOut();
					}
					break;
				}
			case AnimationState.TransitioningOut:
				using (ProfileScope.New("TransitionOut"))
				{
					if (TransitionOut <= 0f || Time >= TransitionOutStartTime + TransitionOut)
					{
						State = AnimationState.Finished;
					}
					break;
				}
			}
		}
	}

	private bool IsJustCreated()
	{
		return CreationTime == Game.Instance.Controllers.TimeController.RealTime;
	}

	protected void SetupMechanicEvents(AnimationClipEvent[] mechanicEventsSorted)
	{
		m_MechanicEvents = mechanicEventsSorted;
		m_NextMechanicEventIndex = 0;
	}

	private void TryFireMechanicEvent()
	{
		while (m_NextMechanicEventIndex < m_MechanicEvents.Length && !(m_MechanicEvents[m_NextMechanicEventIndex].Time > Time))
		{
			AnimationClipEvent e = m_MechanicEvents[m_NextMechanicEventIndex];
			FireMechanicEvent(e);
			m_NextMechanicEventIndex++;
		}
	}

	private bool TryInitInterpolationTime(float? scaledDeltaTime = null)
	{
		if (Mathf.Approximately(m_PreviousTime, m_NextTime))
		{
			m_NextTime = m_PreviousTime + (scaledDeltaTime ?? (GetExpectedDeltaTime(Handle.Manager) * GetSpeed()));
			return true;
		}
		return false;
	}

	private float GetExpectedDeltaTime(IAnimationManager manager)
	{
		return RealTimeController.SystemStepDurationSeconds * ((manager.UpdateMode == DirectorUpdateMode.UnscaledGameTime) ? 1f : (Game.Instance.Controllers.TimeController.TimeScale * manager.PlayingSpeed));
	}
}
