using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpPhaseStatsDetailedView<TViewModel> : CharGenPhaseDetailedView<TViewModel> where TViewModel : CharGenLevelUpBaseStatsPhaseVM<CharGenLevelUpCharacteristicsItemVM>
{
	[SerializeField]
	private CharGenLevelUpStatsSelectorView m_SelectorView;

	[SerializeField]
	private TextMeshProUGUI m_ListHeaderText;

	[SerializeField]
	private OwlcatMultiSelectable m_ListSelectable;

	[SerializeField]
	private TextMeshProUGUI m_RemainingPointsText;

	protected override void OnBind()
	{
		base.OnBind();
		m_SelectorView.Bind(base.ViewModel.SelectionGroup);
		base.ViewModel.IsCompleted.Subscribe(OnComplete).AddTo(this);
		m_RemainingPointsText.transform.parent.gameObject.SetActive(base.ViewModel.SelectionStats.PointsTotal > 1);
		base.ViewModel.RemainingPoints.Subscribe(delegate(int p)
		{
			m_RemainingPointsText.text = p.ToString();
		}).AddTo(this);
	}

	private void OnComplete(bool state)
	{
		LocalizedString localizedString = (state ? base.ViewModel.BlueprintSelectionWithUI.Title : base.ViewModel.BlueprintSelectionWithUI.CallToAction);
		m_ListHeaderText.text = localizedString;
		m_ListSelectable?.SetActiveLayer((!state) ? 1 : 0);
	}
}
