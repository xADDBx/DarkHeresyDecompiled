using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationsView : View<DialogNotificationsVM>
{
	[Serializable]
	private class NotificationGroupWidget
	{
		[SerializeField]
		private GameObject m_GroupParent;

		[SerializeField]
		private DialogNotificationView m_NotificationPrefab;

		[SerializeField]
		private WidgetList m_WidgetList;

		public void SetupNotifications(List<DialogNotificationVM> notifications)
		{
			m_GroupParent.SetActive(notifications.Any());
			m_WidgetList.DrawEntries(notifications, m_NotificationPrefab);
		}

		public void Clear()
		{
			m_WidgetList.Clear();
		}
	}

	[SerializeField]
	private NotificationGroupWidget m_DetectiveNotifications;

	[SerializeField]
	private NotificationGroupWidget m_CommonNotifications;

	[SerializeField]
	private TextMeshProUGUI m_NewItemText;

	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		Clear();
	}

	protected override void OnBind()
	{
		base.ViewModel.PushNotificationsCommand.Subscribe(DisplayNotifications).AddTo(this);
		DisplayNotifications();
		m_NewItemText.text = UIStrings.Instance.Dialog.NewItemLabel;
	}

	private void Clear()
	{
		m_DetectiveNotifications.Clear();
		m_CommonNotifications.Clear();
	}

	private void DisplayNotifications()
	{
		DialogNotificationsVM.NotificationsData notificationsData = base.ViewModel.RequestNotifications();
		bool flag = notificationsData.DetectiveNotifications.Any() || notificationsData.CommonNotifications.Any();
		base.gameObject.SetActive(flag);
		if (flag)
		{
			Clear();
			m_NewItemText.gameObject.SetActive(notificationsData.IsNewItem);
			m_HeaderText.gameObject.SetActive(notificationsData.HeaderText != null);
			m_HeaderText.text = notificationsData.HeaderText;
			m_DetectiveNotifications.SetupNotifications(notificationsData.DetectiveNotifications);
			m_CommonNotifications.SetupNotifications(notificationsData.CommonNotifications);
			NotificationsSounds.Instance.DialogNotifications.SoundList.GetSound(notificationsData.SoundType)?.Play();
		}
	}
}
