using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SystemNotificatorView : View<SystemNotificatorVM>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_NotificationsParent;

	[Header("Views")]
	[SerializeField]
	private SystemNotificationView m_CommonWarning;

	[SerializeField]
	private SystemNotificationView m_AttentionElement;

	[SerializeField]
	private SystemNotificationView m_WarningElement;

	[Header("Values")]
	[SerializeField]
	private int m_MaxNotificationsCount = 3;

	private readonly List<SystemNotificationView> m_Notifications = new List<SystemNotificationView>();

	protected override void OnBind()
	{
		base.ViewModel.CurrentNotification.Skip(1).Subscribe(Show).AddTo(this);
	}

	private void Show(SystemNotificationVM notification)
	{
		m_Notifications.RemoveAll((SystemNotificationView n) => n.ViewModel == null);
		SystemNotificationView systemNotificationView = m_Notifications.FirstOrDefault((SystemNotificationView n) => !n.IsHiding && n.ViewModel.Format == notification.Format && n.ViewModel.Type == notification.Type && n.ViewModel.Label == notification.Label);
		if (systemNotificationView != null)
		{
			systemNotificationView.Refresh();
			return;
		}
		SystemNotificationView systemNotificationView2 = notification.Format switch
		{
			WarningNotificationFormat.Common => WidgetFactory.GetWidget(m_CommonWarning, activate: true, strictMatching: true), 
			WarningNotificationFormat.Attention => WidgetFactory.GetWidget(m_AttentionElement, activate: true, strictMatching: true), 
			WarningNotificationFormat.Warning => WidgetFactory.GetWidget(m_WarningElement, activate: true, strictMatching: true), 
			_ => WidgetFactory.GetWidget(m_CommonWarning, activate: true, strictMatching: true), 
		};
		systemNotificationView2.transform.SetParent(m_NotificationsParent, worldPositionStays: false);
		systemNotificationView2.Bind(notification);
		m_Notifications.Add(systemNotificationView2);
		for (int i = 0; i <= m_Notifications.Count - m_MaxNotificationsCount - 1; i++)
		{
			m_Notifications[i].Hide();
		}
	}
}
