using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickCaseItemView : BrickBaseView<BrickCaseItemVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiSelectable m_ContradictionSelectable;

	protected override void OnBind()
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.CaseItem = base.ViewModel.BlueprintCaseItem;
			m_Description.text = UIStrings.Instance.DetectiveDecor.GetTooltipDesc(base.ViewModel.BlueprintCaseItem);
			m_ContradictionSelectable.SetActiveLayer(base.ViewModel.HasContradiction ? 1 : 0);
		}
	}
}
