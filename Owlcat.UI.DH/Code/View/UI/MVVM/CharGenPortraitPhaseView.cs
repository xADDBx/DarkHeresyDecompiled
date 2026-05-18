using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM;

public class CharGenPortraitPhaseView : CharGenPhaseDetailedView<CharGenPortraitPhaseVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_HeaderLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	[Header("Views")]
	[SerializeField]
	private CharGenPortraitView m_HoverPortraitView;

	[SerializeField]
	private PortraitSelectorCommonView m_PortraitSelector;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.HoverPortrait.Where((PortraitVM p) => p != null).Subscribe(m_HoverPortraitView.Bind).AddTo(this);
		m_PortraitSelector.Bind(base.ViewModel.PortraitSelectorVM);
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
