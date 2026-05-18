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
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpStat : TooltipBaseTemplate
{
	protected readonly StatTooltipData m_StatData;

	protected readonly string m_Name;

	protected readonly string m_Desc;

	protected readonly string m_Acronym;

	private readonly BaseUnitEntity m_Unit;

	private readonly int m_UpgradeProgression;

	private readonly int m_PointsTotal;

	public TooltipTemplateLevelUpStat(StatType statType, BaseUnitEntity unit, int upgradeProgression, int pointsTotal)
	{
		try
		{
			m_Unit = unit;
			m_UpgradeProgression = upgradeProgression;
			m_PointsTotal = pointsTotal;
			m_StatData = StatTooltipData.FromActor(m_Unit, statType);
			StatType stat = (statType.IsAttribute() ? statType : MechanicActor.GetStatBaseStat(statType).GetValueOrDefault());
			m_Acronym = LocalizedTexts.Instance.Stats.GetShortText(stat);
			string key = m_StatData?.KeyWord;
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry(key);
			BlueprintEncyclopediaEntry encyclopediaEntry = UIUtilityEncyclopedy.GetEncyclopediaEntry(key);
			m_Name = string.Empty;
			m_Desc = string.Empty;
			if (encyclopediaEntry != null)
			{
				string text = (from b in encyclopediaEntry.GetTooltipInfo()?.Where((EncyclopediaEntryBlock b) => b.blockType == EncyclopediaEntryBlock.BlockType.Text && b.IsVisibleInTooltip)
					select b.GetDescription()?.Text).FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(text))
				{
					m_Desc = text;
				}
				m_Name = encyclopediaEntry.Title;
			}
			if (glossaryEntry != null)
			{
				if (glossaryEntry.GetDescription().IsSet())
				{
					m_Desc = glossaryEntry.GetDescription();
				}
				m_Name = glossaryEntry.Title;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {statType}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string text = LocalizedTexts.Instance.Stats.GetText(m_StatData.Type.Value);
		string acronym = ((m_StatData.Type.HasValue && m_StatData.Type.Value.IsAttribute()) ? LocalizedTexts.Instance.Stats.GetShortText(m_StatData.Type.Value) : LocalizedTexts.Instance.Stats.GetShortText(MechanicActor.GetStatBaseStat(m_StatData.Type.Value).GetValueOrDefault()));
		LocalizedString localizedString = (m_StatData.Type.Value.IsAttribute() ? UIStrings.Instance.CharGen.Attributes : UIStrings.Instance.CharGen.Skills);
		yield return new BrickChargenStatTitleVM(text, localizedString, acronym, m_StatData.ModifiedValue.ToString());
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddStatBonusesGroup(list);
		list.Add(new BrickSpaceVM(5f));
		AddSkillCheckBonus(list);
		list.Add(new BrickTextVM(m_Desc, TooltipTextType.LevelUpLineSpacing));
		list.Add(new BrickChargenDividerTextLineVM(DividerType.Default));
		AddBonusValue(list);
		list.Add(new BrickSpaceVM(10f));
		AddUpgradeProgression(list);
		return list;
	}

	private void AddSkillCheckBonus(List<ITooltipBrick> result)
	{
		if (m_StatData.BonusValue.HasValue)
		{
			result.Add(new BrickLevelUpSkillcheckBonusVM(UIUtilityText.AddSign(m_StatData.BonusValue.Value)));
		}
	}

	private void AddBonusValue(List<ITooltipBrick> result)
	{
		if (!m_StatData.BonusValue.HasValue)
		{
			return;
		}
		List<StatType> list = (from s in StatTypeHelper.BaseStats
			where s.Value == m_StatData.Type.Value && s.Key.IsSkill()
			select s into st
			select st.Key).ToList();
		if (list.Empty())
		{
			return;
		}
		List<TooltipBrickVM> list2 = new List<TooltipBrickVM>();
		foreach (StatType item in list)
		{
			BrickIconStatValueVM iconStatValue = UIUtilityFeaturesTooltip.GetIconStatValue(item, 10);
			list2.Add(iconStatValue);
		}
		result.Add(new BricksGroupTwoColumnsVM(list2));
	}

	private void AddStatBonusesGroup(List<ITooltipBrick> bricks)
	{
		List<(string, string)> list = new List<(string, string)>();
		if (m_StatData.Group != StatGroup.Skill && m_StatData.BaseValue != 0)
		{
			list.Add((UIStrings.Instance.Tooltips.BaseValue, m_StatData.BaseValue.ToString()));
		}
		list.AddRange(GetStatBonusesGroup(m_StatData.Breakdown.SortedBonuses.Where((StatBonusEntry s) => s.Descriptor.IsPermanentModifier()).ToList()));
		bricks.Add(new BrickLevelUpTitledValueStatGroupVM(UIStrings.Instance.Tooltips.PermanentSources.Text, list));
		List<(string, string)> statBonusesGroup = GetStatBonusesGroup(m_StatData.Breakdown.SortedBonuses.Where((StatBonusEntry s) => !s.Descriptor.IsPermanentModifier()).ToList());
		if (statBonusesGroup.Count != 0)
		{
			bricks.Add(new BrickLevelUpTitledValueStatGroupVM(UIStrings.Instance.Tooltips.TemporarySources.Text, statBonusesGroup));
		}
	}

	private List<(string Name, string Value)> GetStatBonusesGroup(List<StatBonusEntry> bonusEntries)
	{
		List<(string, string)> list = new List<(string, string)>();
		foreach (StatBonusEntry bonusEntry in bonusEntries)
		{
			string item = UIUtilityText.AddSign(bonusEntry.Bonus);
			string text = string.Empty;
			string text2 = string.Empty;
			if (bonusEntry.Descriptor != 0)
			{
				text = ConfigRoot.Instance.LocalizedTexts.Descriptors.GetText(bonusEntry.Descriptor);
			}
			if (!string.IsNullOrWhiteSpace(bonusEntry.Source))
			{
				text2 = bonusEntry.Source;
			}
			string item2 = ((!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2)) ? (text + " [" + text2 + "]") : (string.IsNullOrWhiteSpace(text) ? text2 : text));
			list.Add((item2, item));
		}
		return list;
	}

	private void AddUpgradeProgression(List<ITooltipBrick> result)
	{
		int statPerPoint = ((m_StatData.Group == StatGroup.Skill) ? 10 : 5);
		result.Add(new BrickLevelUpStatProgressionVM(m_UpgradeProgression, statPerPoint, m_PointsTotal));
	}
}
