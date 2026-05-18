using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartNotificationView : View<CombatStartNotificationVM>
{
	[SerializeField]
	private TMP_Text m_ScreenHeaderText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private float m_ShowDurationSec = 2f;

	private IDisposable m_HideTimer;

	protected override void OnBind()
	{
		m_ScreenHeaderText.SetText(base.ViewModel.HeaderText);
		base.ViewModel.CanShow.Subscribe(Show).AddTo(this);
	}

	protected override void OnUnbind()
	{
		StopHideTimer();
		m_FadeAnimator.DisappearInstant();
		base.gameObject.SetActive(value: false);
	}

	private void Show(bool canShow)
	{
		if (!canShow)
		{
			StopHideTimer();
			m_FadeAnimator.DisappearInstant();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
			m_FadeAnimator.PlayAnimation(value: true);
			StartHideTimer();
		}
	}

	private void Hide()
	{
		StopHideTimer();
		m_FadeAnimator.PlayAnimation(value: false, base.ViewModel.Close);
	}

	private void StartHideTimer()
	{
		m_HideTimer?.Dispose();
		m_HideTimer = Observable.Timer(TimeSpan.FromSeconds(m_ShowDurationSec + m_FadeAnimator.AppearTime)).Subscribe(Hide);
	}

	private void StopHideTimer()
	{
		m_HideTimer?.Dispose();
		m_HideTimer = null;
	}
}
