using System;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorItemSelectionVM : CharGenLevelUpSelectorBaseItemVM
{
	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorItemSelectionVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parenNodeVm = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		m_SpriteColor.Value = Color.white;
		base.Template = new TooltipTemplateUIFeature(UIFeature);
	}
}
