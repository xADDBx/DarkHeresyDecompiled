using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateStat : TooltipBaseTemplate
{
	protected readonly StatTooltipData StatData;

	protected readonly string Name;

	protected readonly string Desc;

	private readonly bool m_ShowCompanionStats;

	public TooltipTemplateStat(StatTooltipData statData, bool showCompanionStats = false)
	{
		try
		{
			StatData = statData;
			string key = statData?.KeyWord;
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
			m_ShowCompanionStats = showCompanionStats;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {statData?.Type}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(Name, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickSeparator());
		AddBonusValue(list);
		AddTotalValue(list);
		AddStatBonusesGroup(list);
		AddCompanionsStats(list);
		list.Add(new TooltipBrickText(Desc));
		return list;
	}

	private void AddBonusValue(List<ITooltipBrick> result)
	{
		if (StatData.BonusValue.HasValue)
		{
			result.Add(new TooltipBrickIconValueStat(UIStrings.Instance.Tooltips.BonusValue, UIUtilityText.AddSign(StatData.BonusValue.Value)));
			result.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		}
	}

	private void AddTotalValue(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickIconValueStat(StatData.TotalValueLabel, StatData.ModifiedValue.ToString()));
	}

	private void AddStatBonusesGroup(List<ITooltipBrick> bricks)
	{
		if (StatData.Group != StatGroup.Skill && StatData.BaseValue != 0)
		{
			bricks.Add(new TooltipBrickTextValue(UIStrings.Instance.Tooltips.BaseValue, StatData.BaseValue.ToString(), 1));
		}
		if (!StatData.Breakdown.HasBonuses)
		{
			return;
		}
		foreach (StatBonusEntry sortedBonuse in StatData.Breakdown.SortedBonuses)
		{
			string value = UIUtilityText.AddSign(sortedBonuse.Bonus);
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
			bricks.Add(new TooltipBrickTextValue(empty, value, 1));
		}
	}

	private void AddCompanionsStats(List<ITooltipBrick> bricks)
	{
		if (StatData == null || !m_ShowCompanionStats)
		{
			return;
		}
		BaseUnitEntity currentUnit = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value;
		IEnumerable<BaseUnitEntity> enumerable = from ch in UtilityParty.GetGroup((Game.Instance.LoadedAreaState?.Settings?.CapitalPartyMode).GetValueOrDefault())
			where ch != currentUnit
			select ch;
		if (!enumerable.Any())
		{
			return;
		}
		bricks.Add(new TooltipBricksGroupStart());
		foreach (BaseUnitEntity item in enumerable)
		{
			AddCharacterStatValue(bricks, item);
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddCharacterStatValue(List<ITooltipBrick> bricks, BaseUnitEntity character)
	{
		StatTooltipData statData = StatData;
		if (statData != null && statData.Type.HasValue)
		{
			string value = character.Stats.GetStat(StatData.Type.Value).ModifiedValue.ToString();
			bricks.Add(new TooltipBrickIconStatValue(character.CharacterName, value));
		}
	}
}
