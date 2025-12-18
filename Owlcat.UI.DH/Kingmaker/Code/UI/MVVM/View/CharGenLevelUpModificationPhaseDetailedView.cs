using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpModificationPhaseDetailedView : CharGenLevelUpPhaseSelectionDetailedView<CharGenLevelUpModificationPhaseVM>
{
	[SerializeField]
	private OwlcatMultiButton m_GroupButton;

	[SerializeField]
	private OwlcatMultiButton m_CollapseAll;

	[SerializeField]
	private TextMeshProUGUI m_SourceText;

	[SerializeField]
	private TextMeshProUGUI m_TypeText;

	[SerializeField]
	private TextMeshProUGUI m_CollapseAllText;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsGroupedBySource.Subscribe(delegate(bool v)
		{
			m_GroupButton.SetActiveLayer((!v) ? 1 : 0);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_GroupButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToggleGrouping();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CollapseAll.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.CollapseItemGroups();
		}).AddTo(this);
		m_SourceText.text = UIStrings.Instance.CharGen.GroupBySource;
		m_TypeText.text = UIStrings.Instance.CharGen.GroupByType;
		m_CollapseAllText.text = UIStrings.Instance.CharGen.CollapseAll;
	}
}
