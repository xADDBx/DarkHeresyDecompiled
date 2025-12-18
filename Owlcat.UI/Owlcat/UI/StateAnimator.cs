using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct StateAnimator
{
	private class StateAnimatorTransition : Transition
	{
		private readonly Animator m_Animator;

		private readonly string m_State;

		private readonly bool m_AutoAnimatorToggle;

		private bool m_IsCompleted;

		public override bool Completed => IsCompleted();

		public StateAnimatorTransition(Component host, string state, bool autoAnimatorToggle)
		{
			m_Animator = host.GetComponent<Animator>();
			m_Animator.keepAnimatorStateOnDisable = true;
			m_State = state;
			m_AutoAnimatorToggle = autoAnimatorToggle;
			Play();
		}

		private void Play()
		{
			if (m_AutoAnimatorToggle)
			{
				m_Animator.enabled = true;
			}
			if (m_Animator.isActiveAndEnabled)
			{
				m_Animator.Play(m_State);
				m_Animator.Update(0f);
			}
		}

		private bool IsCompleted()
		{
			if (m_IsCompleted)
			{
				return true;
			}
			if (m_Animator == null)
			{
				return true;
			}
			AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
			if (!m_Animator.isActiveAndEnabled || !currentAnimatorStateInfo.IsName(m_State) || !(currentAnimatorStateInfo.normalizedTime >= 0f) || !(currentAnimatorStateInfo.normalizedTime <= 1f))
			{
				Complete();
			}
			return m_IsCompleted;
		}

		public override void Complete()
		{
			if (!m_IsCompleted)
			{
				m_IsCompleted = true;
				m_Animator.Play(m_State);
				m_Animator.Update(0f);
				m_Animator.Update(m_Animator.GetCurrentAnimatorStateInfo(0).length);
				if (m_AutoAnimatorToggle)
				{
					m_Animator.enabled = false;
				}
			}
		}
	}

	private class AnimatorStateAttribute : PropertyAttribute
	{
		private readonly string m_FieldName;

		public AnimatorStateAttribute(string fieldName)
		{
			m_FieldName = fieldName;
		}
	}

	[SerializeField]
	private Animator m_Animator;

	[SerializeField]
	[AnimatorState("m_Animator")]
	private string m_State;

	public readonly Transition Play(bool autoAnimatorToggle = true)
	{
		return Play(m_Animator, m_State, autoAnimatorToggle);
	}

	public readonly Transition Play(string state, bool autoAnimatorToggle = true)
	{
		return Play(m_Animator, state, autoAnimatorToggle);
	}

	public readonly Transition Play(Component host, string state, bool autoAnimatorToggle = true)
	{
		return new StateAnimatorTransition(host, state, autoAnimatorToggle);
	}
}
