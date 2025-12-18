using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorAbilityItemVM : CharGenLevelUpSelectorBaseItemVM
{
	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorAbilityItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, LevelUpManager levelUpManager, CharGenLevelUpNestedListHeaderVM parenNodeVm = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		if (featureSelectionItem.Feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddFacts) is AddFacts { Facts: var facts } addFacts && facts[0] != null)
		{
			if (addFacts.Facts[0] is BlueprintAbility blueprintAbility)
			{
				base.Template = new TooltipTemplateLevelUpAbility(blueprintAbility, null, null, isScreenWindowTooltip: false, levelUpManager);
				return;
			}
			if (addFacts.Facts[0] is BlueprintToggleAbility ability)
			{
				base.Template = new TooltipTemplateLevelUpToggleAbility(ability, null, levelUpManager);
				return;
			}
		}
		base.Template = new TooltipTemplateUIFeature(UIFeature);
	}
}
