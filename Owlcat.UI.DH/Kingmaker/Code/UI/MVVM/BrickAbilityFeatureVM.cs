using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityFeatureVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly string Acronym;

	public readonly Sprite Icon;

	public readonly Color IconColor;

	public readonly TalentIconInfo TalentIconsInfo;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickAbilityFeatureVM(IUIDataProvider dataProvider, MechanicEntity caster)
	{
		Text = dataProvider.Name;
		if (dataProvider.Icon != null)
		{
			Icon = dataProvider.Icon;
			IconColor = Color.white;
			Acronym = string.Empty;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(dataProvider.NameForAcronym);
			IconColor = UIUtilityText.GetColorByText(dataProvider.NameForAcronym);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(dataProvider.Name);
		}
		if (!(dataProvider is BlueprintAbility blueprintAbility))
		{
			if (!(dataProvider is Ability ability))
			{
				if (dataProvider is BlueprintFeature blueprintFeature)
				{
					Tooltip = new TooltipTemplateFeature(blueprintFeature, withVariants: false, caster);
					TalentIconsInfo = blueprintFeature.TalentIconInfo;
				}
				else
				{
					Tooltip = new TooltipTemplateDataProvider(dataProvider);
				}
			}
			else
			{
				Tooltip = new TooltipTemplateAbility(ability.Data);
			}
		}
		else
		{
			Tooltip = new TooltipTemplateAbility(blueprintAbility);
		}
	}
}
