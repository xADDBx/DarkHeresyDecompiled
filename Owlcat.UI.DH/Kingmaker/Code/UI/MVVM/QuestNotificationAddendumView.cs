using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificationAddendumView : View<QuestNotificationEntityVM>
{
	private bool m_IsInit;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private GameObject m_FailMark;

	[SerializeField]
	private GameObject m_UpdateMark;

	[SerializeField]
	private GameObject m_PostponeMark;

	[SerializeField]
	private GameObject m_CompleteMark;

	[SerializeField]
	private GameObject m_NewMark;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			Clear();
			Hide();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Title.gameObject.SetActive(value: false);
		m_Description.text = base.ViewModel.Description;
		SetMark();
	}

	protected override void OnUnbind()
	{
		Clear();
		Hide();
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Clear()
	{
		m_Title.text = string.Empty;
		m_Description.text = string.Empty;
	}

	private void SetMark()
	{
		m_FailMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Failed);
		m_CompleteMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Completed);
		m_NewMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.New);
		m_UpdateMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Updated);
		m_PostponeMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Postponed);
	}
}
