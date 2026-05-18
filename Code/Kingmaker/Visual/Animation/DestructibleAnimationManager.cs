using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class DestructibleAnimationManager : AnimationManager
{
	private enum State
	{
		Idle,
		Hit,
		Destroyed
	}

	public AnimationClipWrapperSwitcher Idle;

	public AnimationClipWrapperSwitcher OnHit;

	public AnimationClipWrapperSwitcher OnDestroy;

	private State m_CurrentState;

	protected override void OnAfterEnabled()
	{
		m_CurrentState = State.Idle;
		PlayAnimationClip(Idle.GetWrapper(this), 0, 1f, 0.2f, null, ClipDurationType.Default, StartIdleAnimation).NormalizedTime = UnityEngine.Random.value;
	}

	private void StartIdleAnimation()
	{
		m_CurrentState = State.Idle;
		PlayAnimationClip(Idle.GetWrapper(this), 0, 1f, 0.2f, null, ClipDurationType.Default, StartIdleAnimation);
	}

	public void PlayOnHit()
	{
		if (m_CurrentState == State.Idle)
		{
			m_CurrentState = State.Hit;
			PlayAnimationClip(OnHit.GetWrapper(this), 0, 1f, 0.2f, null, ClipDurationType.Default, StartIdleAnimation);
		}
	}

	public void PlayOnDestroy(Action onAnimationEndCallback)
	{
		if (m_CurrentState != State.Destroyed)
		{
			m_CurrentState = State.Destroyed;
			Action onEnd = delegate
			{
				onAnimationEndCallback?.Invoke();
				base.enabled = false;
				m_Animancer.enabled = false;
				m_Animancer.Animator.enabled = false;
			};
			PlayAnimationClip(OnDestroy.GetWrapper(this), 0, 1f, 0.2f, null, ClipDurationType.Default, onEnd);
		}
	}
}
