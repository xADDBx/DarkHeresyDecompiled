using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPostProcessingRandomizer : MonoBehaviour
{
	[SerializeField]
	private List<UIPostEffectState> m_AllowedStates;

	[SerializeField]
	private float m_MinInterval = 1f;

	[SerializeField]
	private float m_MaxInterval = 3f;

	[SerializeField]
	private bool m_AutoStart = true;

	private Coroutine m_Routine;

	private UIPostEffectState m_LastState;

	private bool m_IsRunning;

	private void Start()
	{
		if (m_AutoStart)
		{
			StartRandomizing();
		}
	}

	public void StartRandomizing()
	{
		if (!m_IsRunning && m_AllowedStates.Count != 0)
		{
			m_Routine = StartCoroutine(RandomizeRoutine());
			m_IsRunning = true;
		}
	}

	public void StopRandomizing()
	{
		if (m_IsRunning)
		{
			if (m_Routine != null)
			{
				StopCoroutine(m_Routine);
			}
			m_Routine = null;
			m_IsRunning = false;
		}
	}

	private IEnumerator RandomizeRoutine()
	{
		while (true)
		{
			float time = Random.Range(m_MinInterval, m_MaxInterval);
			yield return new WaitForSecondsRealtime(time);
			if (!(UIPostProcessingAnimator.Instance == null))
			{
				UIPostEffectState uIPostEffectState;
				do
				{
					uIPostEffectState = m_AllowedStates[Random.Range(0, m_AllowedStates.Count)];
				}
				while (m_AllowedStates.Count > 1 && uIPostEffectState.Equals(m_LastState));
				m_LastState = uIPostEffectState;
				UIPostProcessingAnimator.Instance.PlayState(uIPostEffectState);
			}
		}
	}

	private void OnDestroy()
	{
		StopRandomizing();
	}
}
