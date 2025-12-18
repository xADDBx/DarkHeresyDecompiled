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

	protected override BlueprintUISound.UISound GetSound()
	{
		BlueprintUISound sounds = UISounds.Instance.Sounds;
		return base.ViewModel.State switch
		{
			QuestNotificationState.Nothing => sounds.DoNothingEvent, 
			QuestNotificationState.New => sounds.Notifications.NewQuest, 
			QuestNotificationState.Completed => sounds.Notifications.CompletedQuest, 
			QuestNotificationState.Failed => sounds.Notifications.NewInformation, 
			QuestNotificationState.Updated => sounds.Notifications.NewInformation, 
			QuestNotificationState.Postponed => sounds.Notifications.NewInformation, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
