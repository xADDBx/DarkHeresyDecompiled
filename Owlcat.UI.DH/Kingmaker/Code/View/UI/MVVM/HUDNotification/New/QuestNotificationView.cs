using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestNotificationView : QuestNotificationBaseView<QuestNotificationVM>
{
	[Header("Views")]
	[SerializeField]
	private NotificationTitleWithLabelBlockView m_TitleView;

	[SerializeField]
	private NotificationDescriptionBlockView m_DescriptionView;

	[SerializeField]
	private NotificationFooterBlockView m_FooterView;

	protected override void OnBind()
	{
		m_TitleView.Bind(base.ViewModel.TitleBlockVM);
		m_DescriptionView.Bind(base.ViewModel.DescriptionBlockVM);
		m_FooterView.gameObject.SetActive(base.ViewModel.FooterBlockVM != null);
		m_FooterView.Bind(base.ViewModel.FooterBlockVM);
		base.OnBind();
	}

	protected override UISound GetSound()
	{
		NotificationsSounds instance = NotificationsSounds.Instance;
		return base.ViewModel.State switch
		{
			QuestNotificationState.Nothing => UISounds.Instance.Sounds.DoNothingEvent, 
			QuestNotificationState.New => instance.Notifications.NewQuest, 
			QuestNotificationState.Completed => instance.Notifications.CompletedQuest, 
			QuestNotificationState.Failed => instance.Notifications.NewInformation, 
			QuestNotificationState.Updated => instance.Notifications.NewInformation, 
			QuestNotificationState.Postponed => instance.Notifications.NewInformation, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
