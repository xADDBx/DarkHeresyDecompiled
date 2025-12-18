using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UI.UIUtils;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickFeatureVM : TooltipBaseBrickVM
{
	public readonly BlueprintFeature Feature;

	public readonly BlueprintAbility Ability;

	public string Name;

	public Sprite Icon;

	public Color32 IconColor;

	public string Acronym;

	public TooltipBaseTemplate Tooltip;

	public readonly bool IsHeader;

	public bool AvailableBackground;

	public readonly string AdditionalField1;

	public readonly string AdditionalField2;

	public readonly MechanicEntity Caster;

	public readonly TalentIconInfo TalentIconsInfo;

	public readonly bool IsHidden;

	public bool HasFeature => Feature != null;

	public bool HasAbility => Ability != null;

	public TooltipBrickFeatureVM(BlueprintFeature feature, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool forceSetName = false, bool isHidden = false)
	{
		Feature = feature;
		Caster = caster;
		IsHeader = isHeader;
		IsHidden = isHidden;
		string name = feature.Name;
		if (feature != null)
		{
			TalentIconsInfo = feature.TalentIconInfo;
			if (forceSetName && !string.IsNullOrWhiteSpace(feature.ForceSetNameForItemTooltip))
			{
				name = feature.ForceSetNameForItemTooltip;
			}
		}
		Name = name;
		if (showIcon)
		{
			if (feature.Icon != null)
			{
				Icon = feature.Icon;
				IconColor = Color.white;
			}
			else
			{
				Icon = UIUtilityText.GetIconByText(feature.Name);
				IconColor = UIUtilityText.GetColorByText(feature.Name);
				Acronym = UIUtilityAbilities.GetAbilityAcronym(feature);
			}
		}
		AvailableBackground = !available;
		if (!isHeader)
		{
			Tooltip = tooltip ?? new TooltipTemplateFeature(feature, withVariants: false, Caster);
		}
	}

	public TooltipBrickFeatureVM(string name, Sprite icon, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null)
	{
		Caster = caster;
		IsHeader = isHeader;
		Name = name;
		if (showIcon)
		{
			Icon = icon;
			IconColor = Color.white;
		}
		AvailableBackground = !available;
		Tooltip = tooltip;
	}

	public TooltipBrickFeatureVM(BlueprintAbility ability, bool isHeader, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool isHidden = false)
	{
		Ability = ability;
		Caster = caster;
		IsHeader = isHeader;
		IsHidden = isHidden;
		Name = ability.Name;
		if (ability.Icon != null)
		{
			Icon = ability.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(ability.Name);
			IconColor = UIUtilityText.GetColorByText(ability.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(ability.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateAbility(ability, null, caster);
	}

	public TooltipBrickFeatureVM(ToggleAbility ability, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = ability.Name;
		if (ability.Icon != null)
		{
			Icon = ability.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(ability.Name);
			IconColor = UIUtilityText.GetColorByText(ability.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(ability.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateToggleAbility(ability);
	}

	public TooltipBrickFeatureVM(BlueprintToggleAbility ability, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = ability.Name;
		if (ability.Icon != null)
		{
			Icon = ability.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(ability.Name);
			IconColor = UIUtilityText.GetColorByText(ability.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(ability.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateToggleAbility(ability);
	}

	public TooltipBrickFeatureVM(IUIDataProvider dataProvider, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = dataProvider.Name;
		if (dataProvider.Icon != null)
		{
			Icon = dataProvider.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(dataProvider.NameForAcronym);
			IconColor = UIUtilityText.GetColorByText(dataProvider.NameForAcronym);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(dataProvider.Name);
		}
		if (tooltip != null)
		{
			Tooltip = tooltip;
		}
		else if (dataProvider is BlueprintAbility blueprintAbility)
		{
			Tooltip = new TooltipTemplateAbility(blueprintAbility);
		}
		else
		{
			Tooltip = new TooltipTemplateDataProvider(dataProvider);
		}
	}

	public TooltipBrickFeatureVM(UIUtilityItem.UIAbilityData uiAbilityData, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = uiAbilityData.Name;
		if (uiAbilityData.Icon != null)
		{
			Icon = uiAbilityData.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtilityText.GetIconByText(uiAbilityData.Name);
			IconColor = UIUtilityText.GetColorByText(uiAbilityData.Name);
			Acronym = UIUtilityAbilities.GetAbilityAcronym(uiAbilityData.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateAbility(uiAbilityData.BlueprintAbility);
		if (uiAbilityData.BurstAttacksCount > 1)
		{
			AdditionalField2 = string.Format(UIStrings.Instance.Tooltips.ShotsCount, uiAbilityData.BurstAttacksCount.ToString());
		}
	}

	protected TooltipBrickFeatureVM()
	{
	}

	public UnitPartCultAmbush.VisibilityStatuses UpdateCultAmbushVisibility()
	{
		if (HasAbility)
		{
			return Ability.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
		}
		if (HasFeature)
		{
			return Feature.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
		}
		return UnitPartCultAmbush.VisibilityStatuses.Visible;
	}
}
