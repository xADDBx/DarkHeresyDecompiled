using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpFeatureVM : TooltipBrickVM
{
	public readonly LevelUpFeatureUIData UIData;

	public BrickLevelUpFeatureVM(LevelUpFeatureUIData uiData)
	{
		UIData = uiData;
	}

	public BrickLevelUpFeatureVM(BlueprintFeature feature)
	{
		string name = feature.Name;
		string text = null;
		Sprite defaultIfNull = feature.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		TalentIconInfo talentIconInfo = null;
		TooltipBaseTemplate tooltip = null;
		IconDecor iconDecor = IconDecor.Default;
		LevelUpManager levelUpManager = RootVM.Instance.CharGenVM?.CurrentValue?.CharGenContext.LevelUpManager?.CurrentValue;
		if (!feature.FeatureTypes.Empty())
		{
			switch (feature.FeatureTypes?.First())
			{
			case BlueprintFeature.FeatureType.Modifier:
				if (feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddAvailableAbilityModifier) is AddAvailableAbilityModifier addAvailableAbilityModifier && addAvailableAbilityModifier.Modifier != null)
				{
					tooltip = new TooltipTemplateLevelUpModifier(addAvailableAbilityModifier.Modifier, levelUpManager);
					defaultIfNull = addAvailableAbilityModifier.Modifier.Blueprint.Tags.FirstOrDefault()?.Icon;
					TextValueElement title3 = new TextValueElement(name);
					Sprite icon2 = defaultIfNull;
					TooltipBaseTemplate tooltip2 = tooltip;
					UIData = new LevelUpFeatureUIData(title3, null, null, null, icon2, default(Color), IconDecor.Default, null, tooltip2);
					return;
				}
				break;
			case BlueprintFeature.FeatureType.Talent:
			{
				tooltip = new TooltipTemplateLevelUpTalent(new UIFeature(feature), null, levelUpManager);
				TextValueElement title2 = new TextValueElement(feature.Name);
				string abilityAcronym = UtilityAbilities.GetAbilityAcronym(feature.Name);
				TalentIconInfo talentIconInfo2 = feature.TalentIconInfo;
				TooltipBaseTemplate tooltip2 = tooltip;
				UIData = new LevelUpFeatureUIData(title2, abilityAcronym, null, null, null, default(Color), IconDecor.Default, talentIconInfo2, tooltip2);
				return;
			}
			case BlueprintFeature.FeatureType.Specialization:
			{
				tooltip = new TooltipTemplateLevelUpSpecialization(new UIFeature(feature), null, levelUpManager);
				iconDecor = IconDecor.Frame;
				TextValueElement title = new TextValueElement(name);
				Sprite icon = defaultIfNull;
				TooltipBaseTemplate tooltip2 = tooltip;
				IconDecor iconDecor2 = iconDecor;
				UIData = new LevelUpFeatureUIData(title, null, null, null, icon, default(Color), iconDecor2, null, tooltip2);
				return;
			}
			}
		}
		UIData = CheckAbilityAndCreate(feature, name, defaultIfNull, tooltip, levelUpManager);
		if (UIData == null)
		{
			if (feature.Icon == null)
			{
				text = feature.Acronym ?? UtilityAbilities.GetAbilityAcronym(feature.Name);
				talentIconInfo = feature.TalentIconInfo;
			}
			tooltip = new TooltipTemplateFeature(feature, withVariants: false, levelUpManager?.PreviewUnit);
			TextValueElement title4 = new TextValueElement(name);
			string acronym = text;
			Sprite icon3 = defaultIfNull;
			TalentIconInfo talentIconInfo2 = talentIconInfo;
			TooltipBaseTemplate tooltip2 = tooltip;
			IconDecor iconDecor2 = iconDecor;
			UIData = new LevelUpFeatureUIData(title4, acronym, null, null, icon3, default(Color), iconDecor2, talentIconInfo2, tooltip2);
		}
	}

	private LevelUpFeatureUIData CheckAbilityAndCreate(BlueprintFeature feature, string name, Sprite icon, TooltipBaseTemplate tooltip, LevelUpManager levelUpManager)
	{
		if (feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddFacts) is AddFacts { Facts: { Length: >0 }, Facts: var facts2 } addFacts && facts2[0] != null)
		{
			if (addFacts.Facts[0] is BlueprintAbility blueprintAbility)
			{
				tooltip = new TooltipTemplateLevelUpAbility(blueprintAbility, null, null, _: false, levelUpManager);
			}
			else if (addFacts.Facts[0] is BlueprintToggleAbility ability)
			{
				tooltip = new TooltipTemplateLevelUpToggleAbility(ability, null, levelUpManager);
			}
		}
		if (tooltip != null)
		{
			TextValueElement title = new TextValueElement(name);
			TooltipBaseTemplate tooltip2 = tooltip;
			return new LevelUpFeatureUIData(title, null, null, null, icon, default(Color), IconDecor.Default, null, tooltip2);
		}
		return null;
	}
}
