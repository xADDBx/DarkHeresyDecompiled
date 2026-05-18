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
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Localization;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateStat : TooltipBaseTemplate
{
	private readonly bool m_ShowCompanionStats;

	private readonly int m_TotalValue;

	private readonly StatTooltipData m_StatData;

	private LocalizedString m_Name;

	private LocalizedString m_Desc;

	private string Name
	{
		get
		{
			LocalizedString name = m_Name;
			if (name == null)
			{
				return string.Empty;
			}
			return name;
		}
	}

	private string Description
	{
		get
		{
			LocalizedString desc = m_Desc;
			if (desc == null)
			{
				return string.Empty;
			}
			return desc;
		}
	}

	public TooltipTemplateStat(StatTooltipData statData, int totalValueOverride, bool showCompanionStats = false)
		: this(statData, showCompanionStats)
	{
		m_TotalValue = totalValueOverride;
	}

	public TooltipTemplateStat(StatTooltipData statData, bool showCompanionStats = false)
	{
		m_StatData = statData;
		m_TotalValue = statData.ModifiedValue;
		m_ShowCompanionStats = showCompanionStats;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		EnsureTexts();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(new TextEntity(Name, TextFieldParams.Center), TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new BrickSeparatorVM());
		AddBonusValue(list);
		AddTotalValue(list);
		AddStatBonusesGroup(list);
		AddCompanionsStats(list);
		string description = Description;
		if (!string.IsNullOrWhiteSpace(description))
		{
			list.Add(new BrickTextVM(description));
		}
		return list;
	}

	private void AddBonusValue(List<ITooltipBrick> result)
	{
		if (m_StatData.BonusValue.HasValue)
		{
			result.Add(new BrickIconValueStatVM(new TextValueElement(UIStrings.Instance.Tooltips.BonusValue, UIUtilityText.AddSign(m_StatData.BonusValue.Value))));
			result.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
		}
	}

	private void AddTotalValue(List<ITooltipBrick> result)
	{
		string totalValueLabel = m_StatData.TotalValueLabel;
		int totalValue = m_TotalValue;
		result.Add(new BrickIconValueStatVM(new TextValueElement(totalValueLabel, totalValue.ToString())));
	}

	private void AddStatBonusesGroup(List<ITooltipBrick> bricks)
	{
		if (m_StatData.Group != StatGroup.Skill && m_StatData.BaseValue != 0)
		{
			bricks.Add(new BrickTextValueVM(UIStrings.Instance.Tooltips.BaseValue, m_StatData.BaseValue.ToString(), 1));
		}
		if (!m_StatData.Breakdown.HasBonuses)
		{
			return;
		}
		foreach (StatBonusEntry sortedBonuse in m_StatData.Breakdown.SortedBonuses)
		{
			string text = ((sortedBonuse.Descriptor != 0) ? ConfigRoot.Instance.LocalizedTexts.Descriptors.GetText(sortedBonuse.Descriptor) : null);
			string source = sortedBonuse.Source;
			string text2;
			if (text == null)
			{
				text2 = ((source != null) ? source : string.Empty);
			}
			else if (source == null)
			{
				text2 = text;
			}
			else
			{
				string text3 = source;
				text2 = text + " [" + text3 + "]";
			}
			string text4 = text2;
			bricks.Add(new BrickTextValueVM(text4, sortedBonuse.GetStatString(), 1));
		}
	}

	private void AddCompanionsStats(List<ITooltipBrick> bricks)
	{
		if (m_StatData == null || !m_ShowCompanionStats)
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
		List<TooltipBrickVM> list = new List<TooltipBrickVM>();
		foreach (BaseUnitEntity item in enumerable)
		{
			AddCharacterStatValue(list, item);
		}
		bricks.Add(new BricksGroupOneColumnVM(list));
	}

	private void AddCharacterStatValue(List<TooltipBrickVM> bricks, BaseUnitEntity character)
	{
		StatTooltipData statData = m_StatData;
		if (statData != null && statData.Type.HasValue)
		{
			int modifiedValue = character.Actor.GetStat(m_StatData.Type.Value, null, default(StatContext), "AddCharacterStatValue").ModifiedValue;
			string value = modifiedValue.ToString();
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(character.CharacterName, value)));
		}
	}

	private void EnsureTexts()
	{
		if (m_Name != null && m_Desc != null)
		{
			return;
		}
		try
		{
			if (m_StatData == null)
			{
				return;
			}
			string keyWord = m_StatData.KeyWord;
			BlueprintEncyclopediaEntry encyclopediaEntry = UIUtilityEncyclopedy.GetEncyclopediaEntry(keyWord);
			if (encyclopediaEntry != null)
			{
				LocalizedString localizedString = (from b in encyclopediaEntry.GetTooltipInfo()?.Where((EncyclopediaEntryBlock b) => b.blockType == EncyclopediaEntryBlock.BlockType.Text && b.IsVisibleInTooltip)
					select b.GetDescription()).FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(localizedString))
				{
					m_Desc = localizedString;
				}
				m_Name = encyclopediaEntry.Title;
				return;
			}
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry(keyWord);
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
			Debug.LogError(string.Format("Can't resolve {0} texts for: {1}: {2}", "TooltipTemplateStat", m_StatData?.Type, arg));
		}
	}
}
