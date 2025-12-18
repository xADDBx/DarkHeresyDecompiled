using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class QuestNotificatorPCView : QuestNotificatorBaseView
{
	[SerializeField]
	private OwlcatButton m_JournalButton;

	[SerializeField]
	private OwlcatButton m_FullBodyButton;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private TextMeshProUGUI m_ToJournalButtonLabel;

	protected override void OnBind()
	{
		base.OnBind();
		m_CloseButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			Close();
		}).AddTo(this);
		m_JournalButton.OnLeftClickAsObservable().Subscribe(base.OpenJournal).AddTo(this);
		if (m_FullBodyButton != null)
		{
			m_FullBodyButton.OnLeftClickAsObservable().Subscribe(base.OpenJournal).AddTo(this);
		}
		m_ToJournalButtonLabel.text = UIStrings.Instance.QuestNotificationTexts.ToJournal;
	}

	protected override void CheckJournalButtons()
	{
		base.CheckJournalButtons();
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_JournalButton.transform.parent.gameObject.SetActive(CheckActiveToJournalButtons());
			m_FullBodyButton.SetInteractable(CheckActiveToJournalButtons());
		}).AddTo(this);
	}
}
