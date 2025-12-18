using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestAddendumNotificationView : QuestNotificationBaseView<QuestAddendumNotificationVM>
{
	[Header("Views")]
	[SerializeField]
	private NotificationTitleWithLabelBlockView m_TitleView;

	[SerializeField]
	private WidgetList m_ContentWidget;

	[SerializeField]
	private NotificationContentBlockView m_ContentPrefab;

	[SerializeField]
	private NotificationFooterBlockView m_FooterView;

	protected override void OnBind()
	{
		m_TitleView.Bind(base.ViewModel.TitleBlockVM);
		m_ContentWidget.DrawEntries(base.ViewModel.ContentBlockVMs, m_ContentPrefab).AddTo(this);
		m_FooterView.gameObject.SetActive(base.ViewModel.FooterBlockVM != null);
		m_FooterView.Bind(base.ViewModel.FooterBlockVM);
		base.OnBind();
	}

	protected override BlueprintUISound.UISound GetSound()
	{
		return UISounds.Instance.Sounds.Notifications.NewInformation;
	}
}
