using System;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorSpecializationItemVM : CharGenLevelUpSelectorBaseItemVM
{
	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorSpecializationItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, LevelUpManager levelUpManager, CharGenLevelUpNestedListHeaderVM parenNodeVm = null, bool allowSwitchOff = true)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm, allowSwitchOff)
	{
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		base.Template = new TooltipTemplateLevelUpSpecialization(UIFeature, null, levelUpManager);
	}
}
