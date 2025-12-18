using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorModificationItemVM : CharGenLevelUpSelectorBaseItemVM
{
	private readonly LevelUpManager m_LevelUpManager;

	public FeatureSelectionItem FeatureSelectionItem { get; private set; }

	public UIFeature UIFeature { get; private set; }

	public CharGenLevelUpSelectorModificationItemVM(FeatureSelectionItem featureSelectionItem, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parenNodeVm = null, LevelUpManager levelUpManager = null)
		: base(featureSelectionItem.Feature, onHover, parenNodeVm)
	{
		FeatureSelectionItem = featureSelectionItem;
		m_LevelUpManager = levelUpManager;
		UIFeature = new UIFeature(featureSelectionItem.Feature);
		m_Blueprint = featureSelectionItem.Feature;
		m_Acronym.Value = UIUtilityAbilities.GetAbilityAcronym(UIFeature.Name);
		if (featureSelectionItem.Feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddAvailableAbilityModifier) is AddAvailableAbilityModifier addAvailableAbilityModifier && addAvailableAbilityModifier.Modifier != null)
		{
			base.Template = new TooltipTemplateLevelUpModifier(addAvailableAbilityModifier.Modifier, m_LevelUpManager);
			List<string> values = addAvailableAbilityModifier.Modifier.Blueprint.Tags.Select((BlueprintAbilityTag tag) => tag.Name.Text).ToList();
			m_SubLabel.Value = string.Join(", ", values);
			m_Sprite.Value = addAvailableAbilityModifier.Modifier.Blueprint.Tags.FirstOrDefault()?.Icon;
		}
		else
		{
			base.Template = new TooltipTemplateUIFeature(UIFeature);
		}
	}
}
