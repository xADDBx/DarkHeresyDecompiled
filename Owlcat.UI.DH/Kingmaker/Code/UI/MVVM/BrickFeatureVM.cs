using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.UIUtils;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureVM : TooltipBrickVM, IUICultAmbushVisibilityChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly ReactiveProperty<bool> m_IsHidden = new ReactiveProperty<bool>();

	public readonly BlueprintFeature Feature;

	public readonly BlueprintAbility Ability;

	public Color32 IconColor;

	public string Acronym;

	public readonly bool IsHeader;

	public bool AvailableBackground;

	public readonly MechanicEntity Caster;

	public readonly TalentIconInfo TalentIconsInfo;

	public readonly string Name;

	public readonly Sprite Icon;

	public readonly TooltipBaseTemplate Tooltip;

	public bool HasFeature => Feature != null;

	public bool HasAbility => Ability != null;

	public ReadOnlyReactiveProperty<bool> IsHidden => m_IsHidden;

	protected BrickFeatureVM()
	{
	}

	protected BrickFeatureVM(string name, Sprite icon, TooltipBaseTemplate tooltip)
	{
		Name = name;
		Icon = icon;
		Tooltip = tooltip;
	}

	public BrickFeatureVM(BlueprintFeature feature, bool isHeader = false, bool available = true, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool forceSetName = false, bool isHidden = false)
	{
		Feature = feature;
		Caster = caster;
		IsHeader = isHeader;
		m_IsHidden.Value = isHidden;
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

	public BrickFeatureVM(string name, Sprite icon, bool isHeader = false, bool available = true, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null)
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

	public BrickFeatureVM(BlueprintAbility ability, bool isHeader = false, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool isHidden = false)
	{
		Ability = ability;
		Caster = caster;
		IsHeader = isHeader;
		m_IsHidden.Value = isHidden;
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

	public BrickFeatureVM(IUIDataProvider dataProvider, bool isHeader = false, TooltipBaseTemplate tooltip = null)
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
		else if (dataProvider is Ability ability)
		{
			Tooltip = new TooltipTemplateAbility(ability.Data);
		}
		else
		{
			Tooltip = new TooltipTemplateDataProvider(dataProvider);
		}
	}

	public void HandleCultAmbushVisibilityChange()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == Caster)
		{
			bool value = true;
			if (HasAbility)
			{
				value = Ability.CultAmbushVisibility(baseUnitEntity) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
				Ability.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
			}
			else if (HasFeature)
			{
				value = Feature.CultAmbushVisibility(baseUnitEntity) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
				Feature.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
			}
			m_IsHidden.Value = value;
		}
	}
}
