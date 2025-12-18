using System;
using System.Collections;
using UnityEngine;

namespace Owlcat.UI;

internal static class Tween
{
	private class TweenTransition<T1, T2> : Transition, IEnumerator
	{
		private MonoBehaviour m_Host;

		private T1 m_Target;

		private T2 m_Settings;

		private float m_From;

		private float m_To;

		private float m_Duration;

		private Action<T1, T2, float> m_Update;

		private bool m_Completed;

		private Coroutine m_Coroutine;

		private float m_TimeElapsed;

		public override bool Completed => IsCompleted();

		object IEnumerator.Current => null;

		public void Play(MonoBehaviour host, T1 target, T2 settings, float from, float to, float duration, Action<T1, T2, float> update)
		{
			m_Host = host;
			m_Target = target;
			m_Settings = settings;
			m_From = from;
			m_To = to;
			m_Duration = duration;
			m_Update = update;
			m_Completed = false;
			m_TimeElapsed = 0f;
			if (m_Host.isActiveAndEnabled)
			{
				m_Coroutine = m_Host.StartCoroutine(this);
			}
			else
			{
				Complete();
			}
		}

		private bool IsCompleted()
		{
			if (m_Completed)
			{
				return true;
			}
			if (m_Host == null || !m_Host.isActiveAndEnabled)
			{
				Complete();
				return true;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		bool IEnumerator.MoveNext()
		{
			if (m_TimeElapsed < m_Duration)
			{
				float t = Mathf.Clamp01(m_TimeElapsed / m_Duration);
				float arg = Mathf.Lerp(m_From, m_To, t);
				m_Update(m_Target, m_Settings, arg);
				m_TimeElapsed += Time.unscaledDeltaTime;
				return true;
			}
			m_Completed = true;
			m_Update(m_Target, m_Settings, m_To);
			m_Coroutine = null;
			return false;
		}

		public override void Complete()
		{
			if (!m_Completed)
			{
				m_Completed = true;
				m_Update(m_Target, m_Settings, m_To);
				if (m_Coroutine != null && m_Host != null)
				{
					m_Host.StopCoroutine(m_Coroutine);
					m_Coroutine = null;
				}
			}
		}
	}

	public static Transition Play<T1, T2>(MonoBehaviour host, T1 target, T2 settings, float from, float to, float duration, Action<T1, T2, float> update)
	{
		TweenTransition<T1, T2> tweenTransition = new TweenTransition<T1, T2>();
		tweenTransition.Play(host, target, settings, from, to, duration, update);
		return tweenTransition;
	}
}
