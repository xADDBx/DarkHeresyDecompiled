using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpFeature : ITooltipBrick
{
	private readonly TooltipBrickLevelUpFeatureData m_Data;

	public TooltipBrickLevelUpFeature(TooltipBrickLevelUpFeatureData data)
	{
		m_Data = data;
	}

	public TooltipBrickLevelUpFeature(BlueprintFeature feature)
	{
		string name = feature.Name;
		string text = null;
		Sprite sprite = null;
		TalentIconInfo talentIconInfo = null;
		TooltipBaseTemplate tooltipBaseTemplate = null;
		TalentIconInfo talentIconInfo2;
		TooltipBaseTemplate tooltip;
		if (!feature.FeatureTypes.Empty())
		{
			switch (feature.FeatureTypes?.First())
			{
			case BlueprintFeature.FeatureType.Modifier:
				if (feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddAvailableAbilityModifier) is AddAvailableAbilityModifier addAvailableAbilityModifier && addAvailableAbilityModifier.Modifier != null)
				{
					tooltipBaseTemplate = new TooltipTemplateLevelUpModifier(addAvailableAbilityModifier.Modifier, RootVM.Instance.CharGenVM?.CurrentValue?.CharGenContext.LevelUpManager?.CurrentValue);
					sprite = addAvailableAbilityModifier.Modifier.Blueprint.Tags.FirstOrDefault()?.Icon;
					Sprite icon = sprite;
					tooltip = tooltipBaseTemplate;
					m_Data = new TooltipBrickLevelUpFeatureData(name, null, null, null, null, null, icon, iconWithFrame: false, default(Vector2), null, tooltip);
					return;
				}
				break;
			case BlueprintFeature.FeatureType.Talent:
			{
				tooltipBaseTemplate = new TooltipTemplateLevelUpTalent(new UIFeature(feature), null, RootVM.Instance.CharGenVM?.CurrentValue?.CharGenContext.LevelUpManager?.CurrentValue);
				string name2 = feature.Name;
				string abilityAcronym = UtilityAbilities.GetAbilityAcronym(feature.Name);
				talentIconInfo2 = feature.TalentIconInfo;
				tooltip = tooltipBaseTemplate;
				m_Data = new TooltipBrickLevelUpFeatureData(name2, null, abilityAcronym, null, null, null, null, iconWithFrame: true, default(Vector2), talentIconInfo2, tooltip);
				return;
			}
			}
		}
		if (feature.Icon != null)
		{
			sprite = feature.Icon;
		}
		else
		{
			text = feature.Acronym ?? UtilityAbilities.GetAbilityAcronym(feature.Name);
			talentIconInfo = feature.TalentIconInfo;
		}
		tooltipBaseTemplate = new TooltipTemplateFeature(feature, withVariants: false, RootVM.Instance.CharGenVM?.CurrentValue?.CharGenContext.LevelUpManager?.CurrentValue.PreviewUnit);
		string acronym = text;
		Sprite icon2 = sprite;
		talentIconInfo2 = talentIconInfo;
		tooltip = tooltipBaseTemplate;
		m_Data = new TooltipBrickLevelUpFeatureData(name, null, acronym, null, null, null, icon2, iconWithFrame: true, default(Vector2), talentIconInfo2, tooltip);
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpFeatureVM(m_Data);
	}
}
