using System;
using System.Linq;
using Kingmaker.Controllers;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Actions;

public class AnimationActionHandle
{
	private float m_Time;

	private float m_NextTime;

	private float m_SpeedScale = 1f;

	private TimeSpan m_StartTime;

	private bool m_IsReleaseInProgress;

	private int m_ActEventsCounter;

	public IAnimationManager Manager { get; set; }

	public AnimationActionBase Action { get; private set; }

	public AnimationBase ActiveAnimation { get; internal set; }

	public bool IsStarted { get; private set; }

	public UnitAnimationLayerType AnimationLayer { get; set; } = UnitAnimationLayerType.Actions;


	public bool IsFinished { get; private set; }

	public bool IsSkipped { get; set; }

	public bool IsInterrupted { get; private set; }

	public bool PreventsRotation { get; set; }

	public bool HasCrossfadePriority { get; set; }

	public bool SkipFirstTick { get; set; } = true;


	public bool DontReleaseOnInterrupt => Action.DontReleaseOnInterrupt;

	public bool IsReleased { get; private set; }

	public int ActEventsCounter
	{
		get
		{
			return m_ActEventsCounter;
		}
		set
		{
			m_ActEventsCounter = value;
		}
	}

	public bool IsActed => ActEventsCounter > 0;

	public float SpeedScale
	{
		get
		{
			return m_SpeedScale;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, 100f);
			if (!Mathf.Approximately(m_SpeedScale, num))
			{
				m_SpeedScale = num;
				if (ActiveAnimation != null && !Mathf.Approximately(ActiveAnimation.GetSpeed(), m_SpeedScale))
				{
					ActiveAnimation.SetSpeed(m_SpeedScale);
				}
			}
		}
	}

	internal AnimationActionHandle(AnimationActionBase action, IAnimationManager manager)
	{
		Manager = manager;
		Action = action;
	}

	internal void StartInternal()
	{
		if (IsStarted)
		{
			PFLog.Animations.Error("Started animation action handle can't be executed multiple times.");
			return;
		}
		try
		{
			Action.OnStart(this);
		}
		catch (Exception ex)
		{
			PFLog.Animations.Exception(ex);
		}
		IsStarted = true;
		m_Time = 0f;
		m_StartTime = Game.Instance.Controllers.TimeController.RealTime;
		m_NextTime = m_Time + RealTimeController.SystemStepDurationSeconds * SpeedScale;
	}

	internal void FinishInternal()
	{
		if (IsFinished)
		{
			return;
		}
		IsFinished = true;
		try
		{
			Action.OnFinish(this);
		}
		catch (Exception ex)
		{
			PFLog.Animations.Exception(ex);
		}
	}

	internal void UpdateInternal(float deltaTime)
	{
		if (IsJustStarted())
		{
			return;
		}
		m_Time = m_NextTime;
		m_NextTime += deltaTime * SpeedScale;
		try
		{
			Action.OnUpdate(this, deltaTime);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	private bool IsJustStarted()
	{
		return m_StartTime == Game.Instance.Controllers.TimeController.RealTime;
	}

	public float GetTime()
	{
		return m_Time;
	}

	public void StartClip(AnimationClipWrapper clipWrapper, ClipDurationType duration = ClipDurationType.Default)
	{
		AvatarMask avatarMask = Action.AvatarMasks?.FirstOrDefault((AvatarMask am) => am);
		StartClip(clipWrapper, avatarMask, duration);
	}

	public void StartClip(AnimationClipWrapper clipWrapper, AvatarMask avatarMask, ClipDurationType duration)
	{
		Manager.AddAnimationClip(this, clipWrapper, avatarMask, duration);
	}

	public void Release()
	{
		if (!IsReleased && !m_IsReleaseInProgress)
		{
			m_IsReleaseInProgress = true;
			if (ActiveAnimation != null && ActiveAnimation.State != AnimationState.TransitioningOut && ActiveAnimation.State != AnimationState.Finished)
			{
				ActiveAnimation.StartTransitionOut();
			}
			m_IsReleaseInProgress = false;
			IsReleased = true;
		}
	}

	public void ChangeManager(IAnimationManager manager)
	{
		Manager = manager;
		Action = ((UnitAnimationManager)Manager).GetAction(((UnitAnimationAction)Action).Type);
	}

	public void Release(float transisionOut)
	{
		Release();
		if (ActiveAnimation != null)
		{
			ActiveAnimation.TransitionOut = transisionOut;
		}
	}

	internal void MarkInterrupted()
	{
		bool flag2 = (IsFinished = true);
		bool isInterrupted = (IsReleased = flag2);
		IsInterrupted = isInterrupted;
		Action.OnSequencedInterrupted(this);
	}
}
