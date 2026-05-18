using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using UnityEngine;

namespace Kingmaker.Framework.TextTools;

public sealed class UICommonPropertyTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	private static GlossaryColors Colors => ConfigRoot.Instance.UIConfig.PaperGlossaryColors;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!TryGetStatInfo(parameters[0], out var statType, out var encyclopediaKey, out var isBonus, out var isResolve))
		{
			return "UNKNOWN STAT";
		}
		UnitEntity unitEntity = GameLogContext.DescriptionOwner.Value as UnitEntity;
		string glossaryColor = GetGlossaryColor(encyclopediaKey);
		if (unitEntity != null)
		{
			return FormatWithValue(unitEntity, statType, encyclopediaKey, glossaryColor, isBonus, isResolve);
		}
		return FormatWithName(statType, encyclopediaKey, glossaryColor, isResolve);
	}

	private static string FormatWithValue(UnitEntity caster, StatType statType, string encyclopediaKey, string color, bool isBonus, bool isResolve)
	{
		int num = (isBonus ? caster.Actor.GetStatBonus(statType) : caster.Actor.GetStat(statType, null, default(StatContext), "FormatWithValue").ModifiedValue);
		string result = $"<link=\"{encyclopediaKey}\"><color={color}><b>{num}</b></color></link>";
		if (isResolve)
		{
			return $"<u><link=\"{encyclopediaKey}\"><color={color}><b>resolve{num}</b></color></link></u>";
		}
		return result;
	}

	private static string FormatWithName(StatType statType, string encyclopediaKey, string color, bool isResolve)
	{
		string text = LocalizedTexts.Instance.Stats.GetText(statType);
		if (isResolve)
		{
			return "<u><link=\"" + encyclopediaKey + "\"><color=" + color + "><b>resolve" + text + "</b></color></link></u>";
		}
		return "<link=\"" + encyclopediaKey + "\"><color=" + color + "><b>" + text + "</b></color></link>";
	}

	private static string GetGlossaryColor(string encyclopediaKey)
	{
		if (!UtilityLink.CheckLinkKeyHasContent(encyclopediaKey))
		{
			return "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryEmpty);
		}
		if (EntityLink.GetEntityType(UtilityLink.GetKeysFromLink(encyclopediaKey)[0]) == EntityLink.Type.UnitFact)
		{
			return "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryMechanics);
		}
		return "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryGlossary);
	}

	private static bool TryGetStatInfo(string statName, out StatType statType, out string encyclopediaKey, out bool isBonus, out bool isResolve)
	{
		isBonus = false;
		isResolve = false;
		switch (statName)
		{
		case "BallisticSkill":
			statType = StatType.BallisticSkill;
			encyclopediaKey = "Encyclopedia:WarhammerBallisticSkill";
			return true;
		case "WeaponSkill":
			statType = StatType.WeaponSkill;
			encyclopediaKey = "Encyclopedia:WarhammerWeaponSkill";
			return true;
		case "Strength":
			statType = StatType.Strength;
			encyclopediaKey = "Encyclopedia:WarhammerStrength";
			return true;
		case "Toughness":
			statType = StatType.Toughness;
			encyclopediaKey = "Encyclopedia:WarhammerToughness";
			return true;
		case "Agility":
			statType = StatType.Agility;
			encyclopediaKey = "Encyclopedia:WarhammerAgility";
			return true;
		case "Intelligence":
			statType = StatType.Intelligence;
			encyclopediaKey = "Encyclopedia:WarhammerIntelligence";
			return true;
		case "Willpower":
			statType = StatType.Willpower;
			encyclopediaKey = "Encyclopedia:WarhammerWillpower";
			return true;
		case "Perception":
			statType = StatType.Perception;
			encyclopediaKey = "Encyclopedia:WarhammerPerception";
			return true;
		case "Fellowship":
			statType = StatType.Fellowship;
			encyclopediaKey = "Encyclopedia:WarhammerFellowship";
			return true;
		case "BallisticSkillBonus":
			statType = StatType.BallisticSkill;
			encyclopediaKey = "Encyclopedia:WarhammerBallisticSkill";
			isBonus = true;
			return true;
		case "WeaponSkillBonus":
			statType = StatType.WeaponSkill;
			encyclopediaKey = "Encyclopedia:WarhammerWeaponSkill";
			isBonus = true;
			return true;
		case "StrengthBonus":
			statType = StatType.Strength;
			encyclopediaKey = "Encyclopedia:WarhammerStrength";
			isBonus = true;
			return true;
		case "ToughnessBonus":
			statType = StatType.Toughness;
			encyclopediaKey = "Encyclopedia:WarhammerToughness";
			isBonus = true;
			return true;
		case "AgilityBonus":
			statType = StatType.Agility;
			encyclopediaKey = "Encyclopedia:WarhammerAgility";
			isBonus = true;
			return true;
		case "IntelligenceBonus":
			statType = StatType.Intelligence;
			encyclopediaKey = "Encyclopedia:WarhammerIntelligence";
			isBonus = true;
			return true;
		case "WillpowerBonus":
			statType = StatType.Willpower;
			encyclopediaKey = "Encyclopedia:WarhammerWillpower";
			isBonus = true;
			return true;
		case "PerceptionBonus":
			statType = StatType.Perception;
			encyclopediaKey = "Encyclopedia:WarhammerPerception";
			isBonus = true;
			return true;
		case "FellowshipBonus":
			statType = StatType.Fellowship;
			encyclopediaKey = "Encyclopedia:WarhammerFellowship";
			isBonus = true;
			return true;
		case "Resolve":
			statType = StatType.Resolve;
			encyclopediaKey = "Encyclopedia:Resolve";
			isResolve = true;
			return true;
		default:
			statType = StatType.Unknown;
			encyclopediaKey = null;
			return false;
		}
	}
}
