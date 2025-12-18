using System;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestNotificationBaseView<T> : View<T>, IInitializable, INotificationView where T : HUDNotificationNewVM
{
	[Header("Generic")]
	[SerializeField]
	protected FadeAnimator FadeAnimator;

	[SerializeField]
	protected OwlcatMultiSelectable PointerBlocker;

	[SerializeField]
	protected OwlcatMultiButton CardButton;

	[SerializeField]
	protected OwlcatMultiButton CloseButton;

	protected IDisposable HideDisposable;

	public bool IsEmpty => base.ViewModel == null;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		if (base.ViewModel.CanShowButton())
		{
			ObservableSubscribeExtensions.Subscribe(CardButton.OnLeftClickAsObservable(), delegate
			{
				NotificationUtils.OpenJournal();
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(CloseButton.OnLeftClickAsObservable(), delegate
		{
			Hide();
		}).AddTo(this);
		SetupHideTimer();
		PointerBlocker.OnPointerEnterAsObservable().Subscribe(delegate
		{
			HideDisposable?.Dispose();
		}).AddTo(this);
		PointerBlocker.OnPointerExitAsObservable().Subscribe(delegate
		{
			SetupHideTimer();
		}).AddTo(this);
		GetSound().Play();
		FadeAnimator.AppearAnimation();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	protected void SetupHideTimer()
	{
		HideDisposable?.Dispose();
		HideDisposable = NotificationUtils.DoActionAfterDelay(NotificationUtils.Time, Hide).AddTo(this);
	}

	public void Hide()
	{
		FadeAnimator.DisappearAnimation();
		NotificationUtils.DoActionAfterDelay(FadeAnimator.DisappearTime, base.ViewModel.Dispose).AddTo(this);
	}

	protected virtual BlueprintUISound.UISound GetSound()
	{
		return UISounds.Instance.Sounds.DoNothingEvent;
	}
}
