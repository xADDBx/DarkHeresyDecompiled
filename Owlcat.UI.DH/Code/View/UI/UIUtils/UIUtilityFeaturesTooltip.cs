using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Code.View.UI.UIUtils;

public static class UIUtilityFeaturesTooltip
{
	private static readonly FeatureGroup[] DefaultIncludedGroups = new FeatureGroup[4]
	{
		FeatureGroup.ActiveAbility,
		FeatureGroup.Modifier,
		FeatureGroup.Specialization,
		FeatureGroup.ChargenHomeworld
	};

	public static void AddDescription(List<ITooltipBrick> bricks, BlueprintFeature feature, MechanicEntity calculationSource = null)
	{
		if (feature != null && !string.IsNullOrEmpty(feature.Description))
		{
			string description;
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)calculationSource;
				description = feature.Description;
			}
			bricks.Add(new BrickTextVM(description));
		}
	}

	public static void ParseByBlockAndAddFeatures(List<ITooltipBrick> bricks, BlueprintFeature feature, IReadOnlyList<FeatureGroup> includedGroups = null, IReadOnlyList<FeatureGroup> excludedGroups = null)
	{
		if (feature == null)
		{
			return;
		}
		List<BlueprintFeature> list = new List<BlueprintFeature>();
		List<BlueprintFeature> list2 = new List<BlueprintFeature>();
		List<BlueprintFeature> list3 = new List<BlueprintFeature>();
		List<BlueprintFeature> list4 = new List<BlueprintFeature>();
		List<BlueprintFeature> list5 = new List<BlueprintFeature>();
		List<BlueprintFeature> list6 = new List<BlueprintFeature>();
		foreach (AddFacts component2 in feature.GetComponents<AddFacts>())
		{
			foreach (BlueprintUnitFact fact in component2.Facts)
			{
				if (!(fact is BlueprintFeature blueprintFeature))
				{
					continue;
				}
				if (blueprintFeature.FeatureTypes.Contains(BlueprintFeature.FeatureType.Modifier))
				{
					list.Add(blueprintFeature);
				}
				else
				{
					if (blueprintFeature.HideInUI)
					{
						continue;
					}
					if (blueprintFeature.FeatureTypes.Contains(BlueprintFeature.FeatureType.Specialization))
					{
						list2.Add(blueprintFeature);
						continue;
					}
					if (blueprintFeature.FeatureTypes.Contains(BlueprintFeature.FeatureType.Homeworld))
					{
						list3.Add(blueprintFeature);
						continue;
					}
					if (blueprintFeature.FeatureTypes.Contains(BlueprintFeature.FeatureType.Background))
					{
						list4.Add(blueprintFeature);
						continue;
					}
					AddFacts component = blueprintFeature.GetComponent<AddFacts>();
					if (component != null && component.Facts.Any((BlueprintUnitFact fact) => fact is BlueprintAbility))
					{
						list6.Add(blueprintFeature);
					}
					else
					{
						list5.Add(blueprintFeature);
					}
				}
			}
		}
		IReadOnlyList<FeatureGroup> included = includedGroups ?? DefaultIncludedGroups;
		if (IsGroupEnabled(FeatureGroup.ActiveAbility))
		{
			AddFeaturesBlock(bricks, list6, FeatureGroup.ActiveAbility);
		}
		if (IsGroupEnabled(FeatureGroup.Modifier))
		{
			AddFeaturesBlock(bricks, list, FeatureGroup.Modifier);
		}
		if (IsGroupEnabled(FeatureGroup.Specialization))
		{
			AddFeaturesBlock(bricks, list2, FeatureGroup.Specialization);
		}
		if (IsGroupEnabled(FeatureGroup.ChargenHomeworld))
		{
			AddFeaturesBlock(bricks, list3, FeatureGroup.ChargenHomeworld);
		}
		bool IsGroupEnabled(FeatureGroup group)
		{
			if (included.Contains(group))
			{
				if (excludedGroups != null)
				{
					return !excludedGroups.Contains(group);
				}
				return true;
			}
			return false;
		}
	}

	public static void AddFeaturesBlock(List<ITooltipBrick> bricks, List<BlueprintFeature> facts, FeatureGroup featureGroup)
	{
		if (facts.Any())
		{
			bricks.Add(new BrickChargenSectionTitleVM(featureGroup, TitleType.InstantGain));
			bricks.Add(new BricksGroupTwoColumnsVM(((IEnumerable<BlueprintFeature>)facts).Select((Func<BlueprintFeature, TooltipBrickVM>)((BlueprintFeature fact) => new BrickLevelUpFeatureVM(fact))).ToList()));
			bricks.Add(new BrickSpaceVM(2f));
		}
	}

	public static void AddAllLevelUpStatsAndFeatures(List<ITooltipBrick> bricks, BlueprintFeature baseFeature)
	{
		IEnumerable<AddFeaturesToLevelUp> components = baseFeature.GetComponents<AddFeaturesToLevelUp>();
		if (components.Any())
		{
			AddAllLevelUpStatsAndFeatures(bricks, components);
		}
	}

	public static void AddAllLevelUpStatsAndFeatures(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> allFeatures)
	{
		bricks.Add(new BrickChargenDividerTextLineVM(DividerType.Dark, UIStrings.Instance.CharGen.LevelupNeededLabel.Text));
		AddLevelUpStats(bricks, allFeatures, FeatureGroup.Attribute);
		AddLevelUpStats(bricks, allFeatures, FeatureGroup.Skill);
		AddLevelUpFeatures(bricks, allFeatures, FeatureGroup.ActiveAbility);
		AddLevelUpFeatures(bricks, allFeatures, FeatureGroup.Specialization);
		AddLevelUpFeatures(bricks, allFeatures, FeatureGroup.ChargenPsyker);
		AddLevelUpFeatures(bricks, allFeatures, FeatureGroup.Talent);
		AddLevelUpFeatures(bricks, allFeatures, FeatureGroup.Modifier);
	}

	public static void AddLevelUpFeatures(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> allFeatures, FeatureGroup featureGroup)
	{
		if (allFeatures == null)
		{
			return;
		}
		IEnumerable<BlueprintFeature> enumerable = allFeatures.Where((AddFeaturesToLevelUp i) => i.Group == featureGroup).SelectMany((AddFeaturesToLevelUp f) => f.Features);
		if (featureGroup != FeatureGroup.Modifier)
		{
			enumerable = enumerable.Where((BlueprintFeature f) => !f.HideInUI);
		}
		if (!enumerable.Any())
		{
			return;
		}
		bricks.Add(new BrickChargenSectionTitleVM(featureGroup, TitleType.LevelupNeeded));
		List<TooltipBrickVM> list = new List<TooltipBrickVM>();
		foreach (BlueprintFeature item in enumerable)
		{
			list.Add(new BrickLevelUpFeatureVM(item));
		}
		bricks.Add(new BricksGroupTwoColumnsVM(list));
		bricks.Add(new BrickSpaceVM(2f));
	}

	public static bool AddStatBonuses(List<ITooltipBrick> bricks, BlueprintFeature feature, StatType[] types, FeatureGroup featureGroup)
	{
		if (feature == null)
		{
			return false;
		}
		IEnumerable<AddStatModifier> enumerable = from s in feature.GetComponents<AddStatModifier>()
			where types.Contains(s.Restrictions.Stat)
			select s;
		if (enumerable.Empty())
		{
			return false;
		}
		bricks.Add(new BrickChargenSectionTitleVM(featureGroup, TitleType.InstantGain));
		List<TooltipBrickVM> list = new List<TooltipBrickVM>();
		foreach (AddStatModifier item in enumerable)
		{
			BrickIconStatValueVM iconStatValue = GetIconStatValue(item.Restrictions.Stat, item.Value.Value);
			list.Add(iconStatValue);
		}
		bricks.Add(new BricksGroupTwoColumnsVM(list));
		bricks.Add(new BrickSpaceVM(2f));
		return true;
	}

	public static BrickIconStatValueVM GetIconStatValue(StatType stat, int value)
	{
		string statShortLabel = GetStatShortLabel(stat);
		string text = LocalizedTexts.Instance.Stats.GetText(stat);
		string value2 = UIUtilityText.AddSign(value);
		TooltipTemplateGlossary tooltipTemplateGlossary = new TooltipTemplateGlossary(stat.ToString());
		return new BrickIconStatValueVM(new TextValueAddElement(text, value2), tooltip: tooltipTemplateGlossary, iconSize: 40f, iconText: statShortLabel, iconColor: UIConfig.Instance.StatDefaultColor, icon: UIConfig.Instance.UIIcons.StatBackground);
	}

	public static void AddLevelUpStats(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> allFeatures, FeatureGroup featureGroup)
	{
		if (allFeatures == null)
		{
			return;
		}
		IEnumerable<BlueprintFeature> enumerable = allFeatures.Where((AddFeaturesToLevelUp i) => i.Group == featureGroup).SelectMany((AddFeaturesToLevelUp i) => i.Features);
		if (enumerable.Empty())
		{
			return;
		}
		bricks.Add(new BrickChargenSectionTitleVM(featureGroup, TitleType.LevelupNeeded));
		List<TooltipBrickVM> list = new List<TooltipBrickVM>();
		foreach (BlueprintFeature item in enumerable)
		{
			if (item is BlueprintStatAdvancement blueprintStatAdvancement)
			{
				string statShortLabel = GetStatShortLabel(blueprintStatAdvancement.Stat);
				string text = LocalizedTexts.Instance.Stats.GetText(blueprintStatAdvancement.Stat);
				string value = UIUtilityText.AddSign(blueprintStatAdvancement.ValuePerRank);
				TooltipTemplateGlossary tooltipTemplateGlossary = new TooltipTemplateGlossary(blueprintStatAdvancement.Stat.ToString());
				list.Add(new BrickIconStatValueVM(new TextValueAddElement(text, value), tooltip: tooltipTemplateGlossary, iconSize: 54f, iconText: statShortLabel, iconColor: UIConfig.Instance.StatDefaultColor, icon: UIConfig.Instance.UIIcons.StatBackground, type: BrickElementPalette.Positive));
			}
		}
		bricks.Add(new BricksGroupTwoColumnsVM(list));
		bricks.Add(new BrickSpaceVM(2f));
	}

	private static string GetStatShortLabel(StatType statType)
	{
		return UIUtilityText.GetStatShortName(StatDependencyRegistry.Get(statType)?.BaseStat ?? statType);
	}
}
