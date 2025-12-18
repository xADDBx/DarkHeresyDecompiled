using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpStat : TooltipBaseTemplate
{
	protected readonly StatTooltipData StatData;

	protected readonly string Name;

	protected readonly string Desc;

	protected readonly string Acronym;

	private readonly BaseUnitEntity m_Unit;

	private readonly int m_UpgradeProgression;

	public TooltipTemplateLevelUpStat(StatType statType, BaseUnitEntity unit, int upgradeProgression)
	{
		try
		{
			m_Unit = unit;
			m_UpgradeProgression = upgradeProgression;
			if (statType.IsAttribute())
			{
				StatData = new StatTooltipData(m_Unit.Stats.GetAttribute(statType));
				Acronym = LocalizedTexts.Instance.Stats.GetShortText(statType);
			}
			else
			{
				StatData = new StatTooltipData(m_Unit.Stats.GetSkill(statType));
				Acronym = LocalizedTexts.Instance.Stats.GetShortText(m_Unit.Stats.GetSkill(statType).BaseStat.Type);
			}
			string key = StatData?.KeyWord;
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry(key);
			BlueprintEncyclopediaEntry encyclopediaEntry = UIUtilityEncyclopedy.GetEncyclopediaEntry(key);
			Name = string.Empty;
			Desc = string.Empty;
			if (encyclopediaEntry != null)
			{
				string text = (from b in encyclopediaEntry?.GetTooltipInfo()?.Where((EncyclopediaEntryBlock b) => b.blockType == EncyclopediaEntryBlock.BlockType.Text && b.IsVisibleInTooltip)
					select b.GetDescription()?.Text).FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(text))
				{
					Desc = text;
				}
				Name = encyclopediaEntry.Title;
			}
			if (glossaryEntry != null)
			{
				if (glossaryEntry.GetDescription().IsSet())
				{
					Desc = glossaryEntry.GetDescription();
				}
				Name = glossaryEntry.Title;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {statType}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string text = LocalizedTexts.Instance.Stats.GetText(StatData.Type.Value);
		string attribute = ((StatData.Type.HasValue && StatData.Type.Value.IsAttribute()) ? LocalizedTexts.Instance.Stats.GetShortText(StatData.Type.Value) : LocalizedTexts.Instance.Stats.GetShortText(m_Unit.Stats.GetSkill(StatData.Type.Value).BaseStat.Type));
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(text, StatData.ModifiedValue.ToString(), null, null, null, attribute));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddStatBonusesGroup(list);
		list.Add(new TooltipBrickText(Desc, TooltipTextType.LevelUpLineSpacing));
		AddBonusValue(list);
		AddUpgradeProgression(list);
		return list;
	}

	private void AddBonusValue(List<ITooltipBrick> result)
	{
		if (StatData.BonusValue.HasValue)
		{
			List<string> relatedSkills = (from s in StatTypeHelper.BaseStats
				where s.Value == StatData.Type.Value && s.Key.IsSkill()
				select s into st
				select LocalizedTexts.Instance.Stats.GetText(st.Key)).ToList();
			result.Add(new TooltipBrickLevelUpSkillcheckBonus(UIUtilityText.AddSign(StatData.BonusValue.Value), Acronym, relatedSkills));
		}
	}

	private void AddStatBonusesGroup(List<ITooltipBrick> bricks)
	{
		List<(string, string)> list = new List<(string, string)>();
		if (StatData.Group != StatGroup.Skill && StatData.BaseValue != 0)
		{
			list.Add((UIStrings.Instance.Tooltips.BaseValue, StatData.BaseValue.ToString()));
		}
		if (!StatData.Breakdown.HasBonuses)
		{
			return;
		}
		foreach (StatBonusEntry sortedBonuse in StatData.Breakdown.SortedBonuses)
		{
			string item = UIUtilityText.AddSign(sortedBonuse.Bonus);
			string empty = string.Empty;
			string text = string.Empty;
			string text2 = string.Empty;
			if (sortedBonuse.Descriptor != 0)
			{
				text = ConfigRoot.Instance.LocalizedTexts.Descriptors.GetText(sortedBonuse.Descriptor);
			}
			if (!string.IsNullOrWhiteSpace(sortedBonuse.Source))
			{
				text2 = sortedBonuse.Source;
			}
			empty = ((!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2)) ? (text + " [" + text2 + "]") : (string.IsNullOrWhiteSpace(text) ? text2 : text));
			list.Add((empty, item));
		}
		bricks.Add(new TooltipBrickLevelUpTitledValueStatGroup(UIStrings.Instance.Tooltips.Sources.Text, list));
	}

	private void AddUpgradeProgression(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickLevelUpStatProgression(m_UpgradeProgression));
	}
}
