using System;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipConcentrationActionView : View<OvertipConcentrationActionVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private FadeAnimator m_Animator;

	[Space]
	[SerializeField]
	private Image m_ConcentrationBreakMarker;

	[SerializeField]
	private AnimationCurve m_MarkerAnimationCurve;

	[SerializeField]
	private float m_MarkerAnimationDuration;

	private IDisposable m_TooltipIDisposable;

	private Tween m_ConcentrationBreakTween;

	public void HideInstant()
	{
		m_Animator.DisappearInstant();
	}

	protected override void OnBind()
	{
		base.ViewModel.Icon.Subscribe(delegate(Sprite s)
		{
			m_Icon.sprite = s;
		}).AddTo(this);
		base.ViewModel.HasAction.Subscribe(HandleConcentrationChanged).AddTo(this);
		base.ViewModel.CanBreak.Subscribe(HandleBreakStatusChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_ConcentrationBreakTween?.Kill();
		m_ConcentrationBreakTween = null;
	}

	private void HandleConcentrationChanged(bool hasConcentration)
	{
		if (hasConcentration)
		{
			m_Animator.AppearAnimation();
			m_TooltipIDisposable = this.SetTooltip(base.ViewModel.ActionAbilityTooltip).AddTo(this);
		}
		else
		{
			m_Animator.DisappearAnimation();
			m_TooltipIDisposable?.Dispose();
			m_TooltipIDisposable = null;
		}
	}

	private void HandleBreakStatusChanged(bool canBreak)
	{
		if (!canBreak)
		{
			m_ConcentrationBreakTween?.Pause();
			Color color = m_ConcentrationBreakMarker.color;
			color.a = 0f;
			m_ConcentrationBreakMarker.color = color;
		}
		else
		{
			if (m_ConcentrationBreakTween == null)
			{
				m_ConcentrationBreakTween = GetConcentrationMarkerTween().SetAutoKill(autoKillOnCompletion: false);
			}
			m_ConcentrationBreakTween.Play();
		}
	}

	private Tween GetConcentrationMarkerTween()
	{
		Color color = m_ConcentrationBreakMarker.color;
		color.a = 1f;
		m_ConcentrationBreakMarker.color = color;
		return m_ConcentrationBreakMarker.DOFade(0f, m_MarkerAnimationDuration).SetEase(m_MarkerAnimationCurve).SetLoops(-1);
	}
}
