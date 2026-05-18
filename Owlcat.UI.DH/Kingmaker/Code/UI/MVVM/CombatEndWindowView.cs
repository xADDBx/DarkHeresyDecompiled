using System;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatEndWindowView : View<CombatEndWindowVM>, ITransitable
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private ModalWindowView m_ModalWindowView;

	[SerializeField]
	private float m_HideTimer;

	private IDisposable m_HideTimerDisposable;

	Transition ITransitable.Show()
	{
		return new UIAnimatorShowTransition(m_Animator).Run();
	}

	Transition ITransitable.Hide()
	{
		return new UIAnimatorHideTransition(m_Animator).Run();
	}

	protected override void OnBind()
	{
		m_Animator.AppearAnimation();
		m_ModalWindowView.Bind(base.ViewModel.ModalWindowVM);
		if (base.ViewModel.CloseByTimer)
		{
			StartHideTimer();
		}
	}

	protected override void OnUnbind()
	{
		m_ModalWindowView.Unbind();
		StopHideTimer();
	}

	private void StartHideTimer()
	{
		m_HideTimerDisposable?.Dispose();
		TimeSpan dueTime = TimeSpan.FromSeconds(m_HideTimer + m_Animator.AppearTime);
		m_HideTimerDisposable = Observable.Timer(dueTime).Subscribe(Close);
	}

	private void StopHideTimer()
	{
		m_HideTimerDisposable?.Dispose();
		m_HideTimerDisposable = null;
	}

	private void Close()
	{
		base.ViewModel.Close(endCombat: true);
	}
}
