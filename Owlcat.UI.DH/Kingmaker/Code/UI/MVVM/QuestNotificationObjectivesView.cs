using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificationObjectivesView : View<QuestNotificationEntityVM>
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

	[SerializeField]
	private QuestNotificationObjectivesView m_AdditionalObjective;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			Hide();
			m_IsInit = true;
			if (m_AdditionalObjective != null)
			{
				m_AdditionalObjective.Initialize();
			}
		}
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Title.gameObject.SetActive(!string.IsNullOrWhiteSpace(base.ViewModel.Title));
		m_Title.text = base.ViewModel.Title;
		m_Description.gameObject.SetActive(value: false);
		SetMark();
		base.ViewModel.AdditionalObjective.Subscribe(delegate
		{
			BindAdditionalObjective();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void SetMark()
	{
		m_FailMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Failed);
		m_CompleteMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Completed);
		m_NewMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.New);
		m_UpdateMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Updated);
		m_PostponeMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Postponed);
	}

	private void BindAdditionalObjective()
	{
		if (m_AdditionalObjective != null && !string.IsNullOrWhiteSpace(base.ViewModel?.AdditionalObjective?.CurrentValue?.Title))
		{
			m_AdditionalObjective.Bind(base.ViewModel.AdditionalObjective.CurrentValue);
		}
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
