using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerPhaseDetailedView : CharGenPhaseDetailedView<CharGenCareerPhaseVM>
{
	[SerializeField]
	private CharGenCareerSelectorView m_SelectorView;

	[SerializeField]
	private TMP_Text m_HeaderLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_SelectorView.Bind(base.ViewModel.SelectionGroup);
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
