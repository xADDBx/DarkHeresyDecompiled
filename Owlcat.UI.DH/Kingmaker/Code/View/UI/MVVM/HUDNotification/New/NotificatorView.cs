using Kingmaker.Code.UI.MVVM;
using Kingmaker.GameModes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificatorView : View<NotificatorVM>
{
	[Header("Views")]
	[SerializeField]
	private QuestNotificationView m_QuestNotificationView;

	[SerializeField]
	private QuestAddendumNotificationView m_AddendumNotificationView;

	[SerializeField]
	private QuestObjectiveNotificationView m_QuestObjectiveNotificationView;

	[SerializeField]
	private CaseNotificationView m_CaseNotificationView;

	private INotificationView m_CurrentSingleNotifications;

	private void Awake()
	{
		m_QuestNotificationView.Initialize();
		m_AddendumNotificationView.Initialize();
		m_QuestObjectiveNotificationView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		GameUIState.Instance.GameMode.Subscribe(delegate(GameModeType value)
		{
			if (!(value != GameModeType.Cutscene))
			{
				m_CaseNotificationView?.Hide();
				m_CurrentSingleNotifications?.Hide();
			}
		}).AddTo(this);
	}

	private void LateUpdate()
	{
		INotificationView currentSingleNotifications = m_CurrentSingleNotifications;
		if (currentSingleNotifications == null || currentSingleNotifications.IsEmpty)
		{
			HUDNotificationNewVM notificationNewVM = base.ViewModel.TryGetSingleNotification();
			m_CurrentSingleNotifications = ShowNotification(notificationNewVM);
		}
		if (m_CaseNotificationView.ViewModel == null && base.ViewModel.TryGetDetectiveNotification(out var notification))
		{
			m_CaseNotificationView.Bind(notification);
		}
	}

	private INotificationView ShowNotification(HUDNotificationNewVM notificationNewVM)
	{
		if (notificationNewVM == null)
		{
			return null;
		}
		INotificationView result = null;
		if (!(notificationNewVM is QuestNotificationVM source))
		{
			if (!(notificationNewVM is QuestAddendumNotificationVM source2))
			{
				if (notificationNewVM is QuestObjectiveNotificationVM source3)
				{
					m_QuestObjectiveNotificationView.Bind(source3);
					result = m_QuestObjectiveNotificationView;
				}
			}
			else
			{
				m_AddendumNotificationView.Bind(source2);
				result = m_AddendumNotificationView;
			}
		}
		else
		{
			m_QuestNotificationView.Bind(source);
			result = m_QuestNotificationView;
		}
		return result;
	}
}
