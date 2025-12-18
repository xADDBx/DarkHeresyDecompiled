using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpPhaseSelectionDetailedView<TViewModel> : CharGenPhaseDetailedView<TViewModel> where TViewModel : CharGenLevelUpBasePhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	[SerializeField]
	private CharGenLevelUpSelectorView m_SelectorView;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private TextMeshProUGUI m_ListHeaderText;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_SelectorView.Bind(base.ViewModel.SelectionGroup);
		m_ScrollRect?.ScrollToTop();
		base.ViewModel.IsCompleted.Subscribe(OnComplete).AddTo(this);
	}

	private void OnComplete(bool state)
	{
		string text = (state ? UIStrings.Instance.CharGen.GetPhaseName(base.ViewModel.PhaseType) : UIStrings.Instance.CharGen.GetChoosePhaseName(base.ViewModel.PhaseType));
		m_ListHeaderText.text = text;
		m_ListSelectable?.SetActiveLayer((!state) ? 1 : 0);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}
}
