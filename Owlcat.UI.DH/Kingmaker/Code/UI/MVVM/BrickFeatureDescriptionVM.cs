using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureDescriptionVM : TooltipBrickVM
{
	public string Name { get; private set; }

	public string Description { get; private set; }

	public Sprite Icon { get; private set; }

	public Color32 IconColor { get; private set; }

	public string Acronym { get; private set; }

	public TooltipBaseTemplate Tooltip { get; private set; }

	public BrickFeatureDescriptionVM(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
			GameLogContext.DescriptionFactBlueprint = feature;
			Init(feature.Name, feature.Description, feature.Icon, feature, caster);
		}
	}

	public BrickFeatureDescriptionVM(AddKeystoneFeatureInfo featureInfo, MechanicEntity caster = null)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
			GameLogContext.DescriptionFactBlueprint = featureInfo.Feature;
			Init(featureInfo.Title, featureInfo.Description, featureInfo.Icon, featureInfo.Feature, caster);
		}
	}

	private void Init(string name, string description, Sprite sprite, BlueprintFeatureBase feature, MechanicEntity caster)
	{
		Name = name;
		Description = description;
		if (feature.Icon != null)
		{
			Icon = sprite;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(name);
			IconColor = UIUtilityText.GetColorByText(name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(feature);
		}
		Tooltip = GetTooltipTemplate(feature, caster);
	}

	private TooltipBaseTemplate GetTooltipTemplate(BlueprintFeatureBase feature, MechanicEntity caster)
	{
		if (!feature.TryGetAttachableBlueprintFact(out var fact))
		{
			return new TooltipTemplateFeature(feature, withVariants: false, caster);
		}
		if (!(fact is BlueprintAbility blueprintAbility))
		{
			if (fact is BlueprintToggleAbility ability)
			{
				return new TooltipTemplateToggleAbility(ability, caster);
			}
			return new TooltipTemplateFeature(feature, withVariants: false, caster);
		}
		return new TooltipTemplateAbility(blueprintAbility, null, caster);
	}
}
