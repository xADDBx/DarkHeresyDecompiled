using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateCareer : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	private readonly CareerPathUIMetaData m_CareerPathUIMetaData;

	public BlueprintCareerPath BlueprintCareerPath => m_CareerPath.CareerPath;

	public TooltipTemplateCareer(CareerPathVM careerPath, bool _ = false)
	{
		m_CareerPath = careerPath;
		m_CareerPathUIMetaData = careerPath.CareerPath.GetComponent<CareerPathUIMetaData>();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_CareerPath.Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info)
		{
			AddPrerequisites(list);
		}
		list.Add(new BrickTextVM(m_CareerPath.Description, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_CareerPath.Unit));
		AddKeystoneAbilities(list);
		AddUltimateAbilities(list);
		if (type == TooltipTemplateType.Info)
		{
			AddStatsAndSkills(list);
		}
		return list;
	}

	private void AddStatsAndSkills(List<ITooltipBrick> bricks)
	{
		if (Enumerable.Any(m_CareerPath.AdvancementAttributes))
		{
			List<StatType> careerRecommendedStats = UtilityChargen.GetCareerRecommendedStats<BlueprintAttributeAdvancement>(m_CareerPathUIMetaData);
			bricks.Add(new BrickTitleVM(UIStrings.Instance.CharacterSheet.Stats.Text, TooltipTitleType.H5));
			List<TooltipBrickVM> list = new List<TooltipBrickVM>();
			foreach (BlueprintAttributeAdvancement advancementAttribute in m_CareerPath.AdvancementAttributes)
			{
				string text = LocalizedTexts.Instance.Stats.GetText(advancementAttribute.Stat);
				string statShortName = UIUtilityText.GetStatShortName(advancementAttribute.Stat);
				bool isRecommended = careerRecommendedStats.Contains(advancementAttribute.Stat);
				TooltipTemplateStat tooltip = new TooltipTemplateStat(StatTooltipData.FromActor(m_CareerPath.Unit, advancementAttribute.Stat));
				list.Add(new BrickAttributeVM(text, statShortName, tooltip, StripeType.Stat, isRecommended));
			}
			bricks.Add(new BricksGroupTwoColumnsVM(list));
		}
		if (!Enumerable.Any(m_CareerPath.AdvancementSkills))
		{
			return;
		}
		List<StatType> careerRecommendedStats2 = UtilityChargen.GetCareerRecommendedStats<BlueprintSkillAdvancement>(m_CareerPathUIMetaData);
		bricks.Add(new BrickTitleVM(UIStrings.Instance.CharacterSheet.Skills.Text, TooltipTitleType.H5));
		List<TooltipBrickVM> list2 = new List<TooltipBrickVM>();
		foreach (BlueprintSkillAdvancement advancementSkill in m_CareerPath.AdvancementSkills)
		{
			string text2 = LocalizedTexts.Instance.Stats.GetText(advancementSkill.Stat);
			string statShortName2 = UIUtilityText.GetStatShortName(m_CareerPath.Unit.Actor.GetStat(advancementSkill.Stat, null, default(StatContext), "AddStatsAndSkills").BaseStat ?? advancementSkill.Stat);
			bool isRecommended2 = careerRecommendedStats2.Contains(advancementSkill.Stat);
			TooltipTemplateStat tooltip2 = new TooltipTemplateStat(StatTooltipData.FromActor(m_CareerPath.Unit, advancementSkill.Stat));
			list2.Add(new BrickAttributeVM(text2, statShortName2, tooltip2, StripeType.Skill, isRecommended2));
		}
		bricks.Add(new BricksGroupTwoColumnsVM(list2));
	}

	private void AddFeaturesGroup(List<ITooltipBrick> bricks, IEnumerable<BlueprintUnitFact> features, string header)
	{
		if (features.Any())
		{
			bricks.Add(new BrickTitleVM(header, TooltipTitleType.H5));
			bricks.Add(new BricksGroupTwoColumnsVM(features.NotNull().Select((Func<BlueprintUnitFact, TooltipBrickVM>)((BlueprintUnitFact feature) => new BrickFeatureVM(feature))).ToList()));
		}
	}

	private void AddKeystoneAbilities(List<ITooltipBrick> bricks)
	{
		if (m_CareerPathUIMetaData != null)
		{
			List<BlueprintUnitFact> list = new List<BlueprintUnitFact>();
			list.AddRange(m_CareerPathUIMetaData.KeystoneAbilities);
			list.AddRange(m_CareerPathUIMetaData.KeystoneFeatures);
			AddFeaturesGroup(bricks, list, UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader);
		}
	}

	private void AddUltimateAbilities(List<ITooltipBrick> bricks)
	{
		if (m_CareerPathUIMetaData != null)
		{
			IEnumerable<BlueprintAbility> features = m_CareerPathUIMetaData.UltimateFeatures.Select((BlueprintFeature ultimateFeature) => ultimateFeature.GetComponent<AddFacts>()).NotNull().SelectMany((AddFacts i) => i.Facts)
				.Cast<BlueprintAbility>();
			AddFeaturesGroup(bricks, features, UIStrings.Instance.CharacterSheet.UltimateAbilitiesHeader);
		}
	}

	private void AddPrerequisites(List<ITooltipBrick> bricks)
	{
		if (m_CareerPath.Prerequisite != null && !m_CareerPath.IsUnlocked)
		{
			bricks.Add(new BrickPrerequisiteVM(UIUtilityAbilities.GetPrerequisiteEntries(m_CareerPath.Prerequisite)));
		}
	}
}
