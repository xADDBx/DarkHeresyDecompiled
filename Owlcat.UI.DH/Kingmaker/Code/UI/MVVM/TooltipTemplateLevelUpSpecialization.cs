using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpSpecialization : TooltipBaseTemplate
{
	private readonly UIFeature m_UIFeature;

	private readonly bool m_IsInfoWindow;

	private BlueprintFeature m_Feature => m_UIFeature.Feature;

	public TooltipTemplateLevelUpSpecialization(UIFeature feature, bool isInfoWindow = true)
	{
		m_UIFeature = feature;
		m_IsInfoWindow = isInfoWindow;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (!(m_UIFeature.Icon != null))
		{
			UIUtilityAbilities.GetAbilityAcronym(m_UIFeature.Name);
		}
		else
		{
			_ = string.Empty;
		}
		Sprite icon = m_UIFeature.Icon ?? UIUtilityText.GetIconByText(m_UIFeature.NameForAcronym);
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(m_UIFeature.Name, null, null, null, null, null, icon, iconWithFrame: false, new Vector2(84f, 84f)), GetFeatureData());
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddStatBonuses(list, StatTypeHelper.Attributes, UIStrings.Instance.CharGen.BackgroundStatsBonuses);
		AddStatBonuses(list, StatTypeHelper.Skills, UIStrings.Instance.CharGen.BackgroundSkillsBonuses);
		IEnumerable<AddFeaturesToLevelUp> components = m_Feature.GetComponents<AddFeaturesToLevelUp>();
		if (components.Any())
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.CharGen.BackgroundUnlockedFeaturesForLevelUp, TooltipTextType.Centered, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: false, 18, isOverline: true));
			AddLevelUpStats(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Attribute), UIStrings.Instance.CharGen.BackgroundStatsForLevelUp);
			AddLevelUpStats(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Skill), UIStrings.Instance.CharGen.BackgroundSkillsForLevelUp);
			AddLevelUpFeatures(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.ActiveAbility), UIStrings.Instance.CharGen.AbilityTitle);
			AddLevelUpFeatures(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Talent), UIStrings.Instance.CharGen.TalentTitle);
			AddLevelUpFeatures(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Modifier), UIStrings.Instance.CharGen.ModificationTitle);
		}
		list.Add(new TooltipBrickSpace());
		return list;
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Feature.Description))
		{
			string text = UIUtilityText.UpdateDescriptionWithUIProperties(m_Feature.Description, null);
			bricks.Add(new TooltipBrickText(text));
		}
	}

	private TooltipBrickLevelUpFeatureData GetFeatureData()
	{
		BlueprintUnitFact blueprintUnitFact = m_Feature.GetComponents<AddFacts>().SelectMany((AddFacts i) => i.Facts).FirstOrDefault();
		if (blueprintUnitFact == null)
		{
			return null;
		}
		Sprite icon = blueprintUnitFact.Icon;
		string acronym = ((icon == null) ? UIUtilityAbilities.GetAbilityAcronym(blueprintUnitFact.LocalizedName) : null);
		string title = blueprintUnitFact.LocalizedName;
		Sprite icon2 = icon;
		return new TooltipBrickLevelUpFeatureData(title, null, acronym, null, null, null, icon2);
	}

	private void AddStatBonuses(List<ITooltipBrick> bricks, StatType[] types, string title)
	{
		IEnumerable<AddStatModifier> enumerable = from s in m_Feature.GetComponents<AddStatModifier>()
			where types.Contains(s.Stat)
			select s;
		if (!enumerable.Any())
		{
			return;
		}
		List<(string, string)> list = new List<(string, string)>();
		foreach (AddStatModifier item2 in enumerable)
		{
			string text = LocalizedTexts.Instance.Stats.GetText(item2.Stat);
			string item = UIUtilityText.AddSign(item2.Value.Value);
			list.Add((text, item));
		}
		bricks.Add(new TooltipBrickLevelUpTitledValueStatGroup(title + ":", list));
	}

	private string GetStatShortLabel(StatType statType)
	{
		return UIUtilityText.GetStatShortName(UIUtilityUnit.GetSourceStatType(Game.Instance.Player.MainCharacterEntity.Stats.Container.GetStat(statType)) ?? statType);
	}

	private void AddLevelUpStats(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> levelUpFeatures, string title)
	{
		if (!levelUpFeatures.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H3));
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false));
		foreach (BlueprintFeature item in levelUpFeatures.SelectMany((AddFeaturesToLevelUp i) => i.Features))
		{
			if (item is BlueprintStatAdvancement blueprintStatAdvancement)
			{
				string statShortLabel = GetStatShortLabel(blueprintStatAdvancement.Stat);
				string text = LocalizedTexts.Instance.Stats.GetText(blueprintStatAdvancement.Stat);
				string text2 = UIUtilityText.AddSign(blueprintStatAdvancement.ValuePerRank);
				TooltipTemplateGlossary tooltipTemplateGlossary = new TooltipTemplateGlossary(blueprintStatAdvancement.Stat.ToString());
				bricks.Add(new TooltipBrickIconStatValue(text, text2, null, tooltip: tooltipTemplateGlossary, iconSize: 40f, iconText: statShortLabel, iconColor: UIConfig.Instance.StatPositiveColor, icon: UIConfig.Instance.UIIcons.StatBackground, type: TooltipBrickIconStatValueType.Positive));
			}
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddLevelUpFeatures(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> levelUpFeatures, string title)
	{
		if (!levelUpFeatures.Any())
		{
			return;
		}
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, title: title, layoutParams: new TooltipBricksGroupLayoutParams
		{
			Spacing = new Vector2(8f, 0f)
		}));
		foreach (AddFeaturesToLevelUp levelUpFeature in levelUpFeatures)
		{
			foreach (BlueprintFeature feature in levelUpFeature.Features)
			{
				bricks.Add(new TooltipBrickLevelUpFeature(feature));
			}
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}
}
