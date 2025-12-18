using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipConcentrationActionView : View<OvertipConcentrationActionVM>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private FadeAnimator m_Animator;

	private IDisposable m_TooltipIDisposable;

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
		base.ViewModel.HasAction.Subscribe(delegate(bool b)
		{
			if (b)
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
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
	}
}
