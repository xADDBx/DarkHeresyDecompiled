using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Framework;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorAbilityUpgradeItemVM : CharGenLevelUpSelectorBaseItemVM
{
	private readonly LevelUpManager m_LevelUpManager;

	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public IUIDataProvider BaseAbilityData { get; private set; }

	public CharGenLevelUpSelectorAbilityUpgradeItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, LevelUpManager levelUpManager, CharGenLevelUpNestedListHeaderVM parenNodeVm = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		m_LevelUpManager = levelUpManager;
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		if (!(featureSelectionItem.Feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddFacts) is AddFacts { Facts: var facts } addFacts))
		{
			base.Template = new TooltipTemplateUIFeature(UIFeature);
		}
		else if (facts[0] != null && addFacts.Facts[0] is BlueprintAbility blueprintAbility)
		{
			BlueprintAbility blueprintAbility2 = null;
			if (blueprintAbility.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is UpgradeAbility) is UpgradeAbility upgradeAbility)
			{
				blueprintAbility2 = upgradeAbility.BaseAbility;
			}
			BaseAbilityData = blueprintAbility2;
			base.Template = new TooltipTemplateLevelUpAbilityUpgrade(blueprintAbility, blueprintAbility2, null, m_LevelUpManager);
		}
		else if (addFacts.Facts[0] != null && addFacts.Facts[0] is BlueprintToggleAbility blueprintToggleAbility)
		{
			BlueprintToggleAbility blueprintToggleAbility2 = null;
			if (blueprintToggleAbility.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is UpgradeToggleAbility) is UpgradeToggleAbility upgradeToggleAbility)
			{
				blueprintToggleAbility2 = upgradeToggleAbility.BaseAbility;
			}
			BaseAbilityData = blueprintToggleAbility2;
			base.Template = new TooltipTemplateLevelUpAbilityUpgrade(blueprintToggleAbility, blueprintToggleAbility2, null, m_LevelUpManager);
		}
	}
}
