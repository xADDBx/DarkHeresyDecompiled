using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class MoveSequenceAnimator : MonoBehaviour, IUIAnimator
{
	[Serializable]
	public class MoveStep
	{
		public Vector2 Position;

		public float Time = 0.2f;

		public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	}

	[SerializeField]
	private float m_AppearDelay;

	[SerializeField]
	private float m_DisappearDelay;

	[Tooltip("Если true — все точки будут смещены относительно начальной позиции RectTransform")]
	public bool Relatively = true;

	[Tooltip("Шаг 0 — Appear позиция, последний шаг — Disappear. Между ними — промежуточные.")]
	[SerializeField]
	private List<MoveStep> m_Steps = new List<MoveStep>();

	private bool m_IsInit;

	private RectTransform m_RectTransform;

	[HideInInspector]
	public bool isAnimating;

	private RectTransform RectTransform
	{
		get
		{
			if (!m_RectTransform)
			{
				m_RectTransform = GetComponent<RectTransform>();
			}
			return m_RectTransform;
		}
	}

	public float AnimationTime
	{
		get
		{
			if (m_Steps == null || m_Steps.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < m_Steps.Count; i++)
			{
				num += m_Steps[i].Time;
			}
			return num;
		}
	}

	public void Initialize()
	{
		if (m_IsInit)
		{
			return;
		}
		Vector2 anchoredPosition = RectTransform.anchoredPosition;
		if (m_Steps == null)
		{
			m_Steps = new List<MoveStep>();
		}
		if (Relatively && m_Steps.Count > 0)
		{
			for (int i = 0; i < m_Steps.Count; i++)
			{
				m_Steps[i].Position += anchoredPosition;
			}
		}
		if (m_Steps.Count > 0)
		{
			anchoredPosition = m_Steps[m_Steps.Count - 1].Position;
			RectTransform.anchoredPosition = anchoredPosition;
		}
		m_IsInit = true;
	}

	public void AppearAnimation(UnityAction action = null)
	{
		Initialize();
		if (m_Steps == null || m_Steps.Count == 0)
		{
			action?.Invoke();
			return;
		}
		DOTween.Kill(RectTransform);
		RectTransform.anchoredPosition = m_Steps[m_Steps.Count - 1].Position;
		Sequence s = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetDelay(m_AppearDelay)
			.SetTarget(RectTransform)
			.OnComplete(delegate
			{
				action?.Invoke();
				action = null;
			});
		for (int num = m_Steps.Count - 2; num >= 0; num--)
		{
			MoveStep moveStep = m_Steps[num];
			s.Append(RectTransform.DOAnchorPos(moveStep.Position, moveStep.Time).SetEase(moveStep.Curve).SetUpdate(isIndependentUpdate: true));
		}
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		Initialize();
		if (m_Steps == null || m_Steps.Count == 0)
		{
			action?.Invoke();
			return;
		}
		DOTween.Kill(RectTransform);
		RectTransform.anchoredPosition = m_Steps[0].Position;
		Sequence s = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetDelay(m_DisappearDelay)
			.SetTarget(RectTransform)
			.OnComplete(delegate
			{
				action?.Invoke();
				action = null;
			});
		for (int i = 1; i < m_Steps.Count; i++)
		{
			MoveStep moveStep = m_Steps[i];
			s.Append(RectTransform.DOAnchorPos(moveStep.Position, moveStep.Time).SetEase(moveStep.Curve).SetUpdate(isIndependentUpdate: true));
		}
	}

	public void PlayAnimation(bool value, UnityAction action = null)
	{
		if (value)
		{
			AppearAnimation(action);
		}
		else
		{
			DisappearAnimation(action);
		}
	}

	public void SetAppearPosition(Vector2 position)
	{
		EnsureStepsCountAtLeast(1);
		m_Steps[0].Position = position;
	}

	public void SetDisappearPosition(Vector2 position)
	{
		EnsureStepsCountAtLeast(2);
		m_Steps[m_Steps.Count - 1].Position = position;
	}

	public void SetDisappearDelay(float delay)
	{
		m_DisappearDelay = delay;
	}

	private void EnsureStepsCountAtLeast(int count)
	{
		if (m_Steps == null)
		{
			m_Steps = new List<MoveStep>();
		}
		while (m_Steps.Count < count)
		{
			m_Steps.Add(new MoveStep());
		}
	}

	public Vector2 GetDisappearPosition()
	{
		return m_Steps?.LastOrDefault()?.Position ?? Vector2.zero;
	}

	public Vector2 GetAppearPosition()
	{
		return m_Steps?.FirstOrDefault()?.Position ?? Vector2.zero;
	}
}
