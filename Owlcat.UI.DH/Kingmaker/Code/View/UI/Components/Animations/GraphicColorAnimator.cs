using System;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Animations;

public class GraphicColorAnimator : MonoBehaviour, IUIAnimator
{
	[Serializable]
	private struct AnimationParams
	{
		public Color Color;

		public float Duration;

		public AnimationCurve Curve;
	}

	[SerializeField]
	private Graphic m_Graphic;

	[SerializeField]
	private AnimationParams m_AppearParams;

	[SerializeField]
	private AnimationParams m_DisappearParams;

	private Tween m_AppearTween;

	private Tween m_DisappearTween;

	private UnityAction m_OnComplete;

	public void Initialize()
	{
	}

	public void AppearAnimation(UnityAction action = null)
	{
		m_OnComplete = action;
		m_AppearTween?.Pause();
		m_DisappearTween?.Pause();
		if (m_AppearTween == null)
		{
			m_AppearTween = m_Graphic.DOColor(m_AppearParams.Color, m_AppearParams.Duration).SetAutoKill(autoKillOnCompletion: false).SetEase(m_AppearParams.Curve)
				.OnComplete(delegate
				{
					m_OnComplete?.Invoke();
					m_OnComplete = null;
				});
		}
		m_AppearTween.Restart();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		m_OnComplete = action;
		m_AppearTween?.Pause();
		m_DisappearTween?.Pause();
		if (m_DisappearTween == null)
		{
			m_DisappearTween = m_Graphic.DOColor(m_DisappearParams.Color, m_DisappearParams.Duration).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearParams.Curve)
				.OnComplete(delegate
				{
					m_OnComplete?.Invoke();
					m_OnComplete = null;
				});
		}
		m_DisappearTween.Restart();
	}

	private void OnDestroy()
	{
		m_DisappearTween?.Kill();
		m_AppearTween?.Kill();
		m_DisappearTween = null;
		m_AppearTween = null;
		m_OnComplete = null;
	}
}
