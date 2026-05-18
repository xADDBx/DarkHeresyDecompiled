using Animancer;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class PlayableInfo : AnimationBase
{
	private AnimancerState m_AnimancerState;

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

	internal PlayableInfo(AnimationActionHandle handle, AnimancerState animancerState, AnimationClipEvent[] mechanicEventsSorted)
		: base(handle)
	{
		m_AnimancerState = animancerState;
		Reset(handle);
		SetupMechanicEvents(mechanicEventsSorted);
	}

	internal void Reset(AnimationActionHandle handle)
	{
		base.Handle = handle;
		base.CreationTime = Game.Instance.Controllers.TimeController.RealTime;
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

	public sealed override AnimationClip GetPlayableClip()
	{
		return m_AnimancerState.Clip;
	}

	public override AnimationClip GetActiveClip()
	{
		return m_AnimancerState.Clip;
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

	protected override void FireMechanicEvent(AnimationClipEvent e)
	{
		e.Start(base.Handle.Manager);
	}

	public override void RemoveFromManager()
	{
	}

	public override AnimationBase Find(AvatarMask mask, bool isAdditive)
	{
		return null;
	}
}
