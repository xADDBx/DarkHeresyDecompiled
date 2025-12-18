using System;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorSpecializationItemVM : CharGenLevelUpSelectorBaseItemVM
{
	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorSpecializationItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parenNodeVm = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		base.Template = new TooltipTemplateLevelUpSpecialization(UIFeature);
	}
}
