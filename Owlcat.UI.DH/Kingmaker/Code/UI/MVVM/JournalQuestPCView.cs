using Kingmaker.UI.Common;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestPCView : BaseJournalItemPCView
{
	[Header("Header")]
	[SerializeField]
	private TextMeshProUGUI m_TitleLabel;

	[Header("Location Info")]
	[SerializeField]
	private TextMeshProUGUI m_PlaceLabel;

	[Header("Completion")]
	[SerializeField]
	private GameObject m_CompletionItem;

	[SerializeField]
	private TextMeshProUGUI m_CompletionLabel;

	[Header("Content")]
	[SerializeField]
	private TextMeshProUGUI m_ServiceMessageLabel;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[Header("Objectives")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private RectTransform m_LocationGroup;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateStatusCommand, delegate
		{
			SetupStatuses();
		}).AddTo(this);
	}

	protected override void UpdateView()
	{
		SetupHeader();
		SetupBody();
		ScrollToTop();
		base.UpdateView();
	}

	private void SetupHeader()
	{
		m_TitleLabel.text = base.ViewModel.Title;
		m_LocationGroup.gameObject.SetActive(!string.IsNullOrWhiteSpace(base.ViewModel.Place));
		m_PlaceLabel.text = base.ViewModel.Place;
	}

	private void SetupBody()
	{
		SetTextItem(m_ServiceMessageLabel.gameObject, m_ServiceMessageLabel, base.ViewModel.ServiceMessage);
		SetTextItem(m_DescriptionLabel.gameObject, m_DescriptionLabel, base.ViewModel.Description);
		SetTextItem(m_CompletionItem, m_CompletionLabel, base.ViewModel.CompletionText);
		SetupStatuses();
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}
}
