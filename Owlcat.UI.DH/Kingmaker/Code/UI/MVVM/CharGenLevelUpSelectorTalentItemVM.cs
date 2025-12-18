using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorTalentItemVM : CharGenLevelUpSelectorBaseItemVM
{
	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorTalentItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, LevelUpManager levelUpManager, CharGenLevelUpNestedListHeaderVM parenNodeVm = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		FeatureSelectionItem = featureSelectionItem;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		base.Template = new TooltipTemplateLevelUpTalent(UIFeature, null, levelUpManager);
		m_Sprite.Value = ConfigRoot.Instance.UIConfig.ModifierColors.DefaultSprite;
		m_SpriteColor.Value = UIUtilityText.GetColorByText(UIFeature.Name);
	}
}
