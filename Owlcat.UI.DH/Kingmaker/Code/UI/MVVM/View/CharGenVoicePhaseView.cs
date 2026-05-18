using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoicePhaseView : CharGenPhaseDetailedView<CharGenVoicePhaseVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private CharGenVoiceSelectorGroupView m_VoiceSelectorGroupView;

	[SerializeField]
	private CharGenVoicePhraseButtonsView m_PhraseButtonsView;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_HeaderLabel.text = base.ViewModel.PhaseName.CurrentValue;
		if (m_VoiceSelectorGroupView != null)
		{
			m_VoiceSelectorGroupView.Bind(base.ViewModel.VoiceSelector);
		}
		if (m_PhraseButtonsView != null)
		{
			m_PhraseButtonsView.Bind(base.ViewModel.PhraseButtons);
			base.ViewModel.SelectedVoiceVM.Subscribe(delegate(CharGenVoiceItemVM v)
			{
				m_PhraseButtonsView.gameObject.SetActive(v != null);
			}).AddTo(this);
		}
		base.ViewModel.IsCompleted.Subscribe(OnComplete).AddTo(this);
	}

	private void OnComplete(bool state)
	{
		string text = ((base.ViewModel.BlueprintSelectionWithUI == null) ? base.ViewModel.PhaseName.CurrentValue : ((string)(state ? base.ViewModel.BlueprintSelectionWithUI.Title : base.ViewModel.BlueprintSelectionWithUI.CallToAction)));
		if ((bool)m_HeaderLabel)
		{
			m_HeaderLabel.text = text;
		}
		if ((bool)m_ListSelectable)
		{
			m_ListSelectable.SetActiveLayer((!state) ? 1 : 0);
		}
	}
}
