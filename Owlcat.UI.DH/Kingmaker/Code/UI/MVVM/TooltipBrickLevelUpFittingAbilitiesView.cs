using Assets.Code.View.UI.MVVM;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpFittingAbilitiesView : TooltipBaseBrickView<TooltipBrickLevelUpFittingAbilitiesVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_NoAbilities;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private TooltipElementAbilityWithModifierView m_AbilityWithModifierPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.text = UIStrings.Instance.Tooltips.FittingAbilities;
		m_NoAbilities.text = UIStrings.Instance.Tooltips.NoSuitableAbilities;
		m_NoAbilities.gameObject.SetActive(base.ViewModel.Abilities.Empty());
		m_WidgetList.DrawEntries(base.ViewModel.Abilities, m_AbilityWithModifierPrefab).AddTo(this);
	}
}
