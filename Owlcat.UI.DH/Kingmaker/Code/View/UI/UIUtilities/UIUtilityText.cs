using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps.Simple;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityText
{
	public static UITooltips UITooltips => LocalizedTexts.Instance.UserInterfacesText.Tooltips;

	public static string GetCostFormatted(float value)
	{
		return value.ToString("0.#");
	}

	public static string GetStatShortName(StatType statType)
	{
		UIStrings userInterfacesText = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText;
		return statType switch
		{
			StatType.WeaponSkill => userInterfacesText.CharacterSheet.WeaponSkillShort.Text, 
			StatType.BallisticSkill => userInterfacesText.CharacterSheet.BallisticSkillShort.Text, 
			StatType.Strength => userInterfacesText.CharacterSheet.StrengthShort.Text, 
			StatType.Toughness => userInterfacesText.CharacterSheet.ToughnessShort.Text, 
			StatType.Agility => userInterfacesText.CharacterSheet.AgilityShort.Text, 
			StatType.Intelligence => userInterfacesText.CharacterSheet.InteligenceShort.Text, 
			StatType.Perception => userInterfacesText.CharacterSheet.PerceptionShort.Text, 
			StatType.Willpower => userInterfacesText.CharacterSheet.WillpowerShort.Text, 
			StatType.Fellowship => userInterfacesText.CharacterSheet.FellowshipShort.Text, 
			_ => string.Empty, 
		};
	}

	public static string AddPercentTo(string text)
	{
		return string.Format(UIConfig.Instance.TextFormats.PercentFormat, text);
	}

	public static string AddPercentTo(int value)
	{
		return AddPercentTo(value.ToString());
	}

	public static string GetTrapSkillCheckText(DisableTrapInteractionPart trap, List<BaseUnitEntity> units)
	{
		if (trap == null)
		{
			return string.Empty;
		}
		SimpleTrapObjectView simpleTrapObjectView = trap.View as SimpleTrapObjectView;
		SimpleTrapObjectInfo simpleTrapObjectInfo = simpleTrapObjectView?.Info;
		if (simpleTrapObjectInfo == null)
		{
			return string.Empty;
		}
		StatType disarmSkill = simpleTrapObjectInfo.DisarmSkill;
		string text = LocalizedTexts.Instance.Stats.GetText(disarmSkill);
		int disableDC = simpleTrapObjectView.Data.DisableDC;
		int interactionSkillCheckChance = InteractionHelper.GetInteractionSkillCheckChance(trap.SelectUnit(units), disarmSkill, disableDC);
		return $"[{text}: {interactionSkillCheckChance}%]";
	}

	public static string GetPercentString(float value)
	{
		if (!(value >= 0f))
		{
			return "";
		}
		return $"{Mathf.Round(value)}%";
	}

	public static string Capitalize(this string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		return char.ToUpper(text[0]) + text.Substring(1, text.Length - 1);
	}

	public static string GetModifierString(int value)
	{
		if (value <= 0)
		{
			return value.ToString();
		}
		return "+" + value;
	}

	public static string GetQuestionSprite()
	{
		return "<sprite name=\"UI_Question\">";
	}

	public static string WrapWithWeight(string text, TextFontWeight weight)
	{
		return weight switch
		{
			TextFontWeight.Thin => "<font-weight=\"100\">" + text + "</font-weight>", 
			TextFontWeight.ExtraLight => "<font-weight=\"200\">" + text + "</font-weight>", 
			TextFontWeight.Light => "<font-weight=\"300\">" + text + "</font-weight>", 
			TextFontWeight.Regular => "<font-weight=\"400\">" + text + "</font-weight>", 
			TextFontWeight.Medium => "<font-weight=\"500\">" + text + "</font-weight>", 
			TextFontWeight.SemiBold => "<font-weight=\"600\">" + text + "</font-weight>", 
			TextFontWeight.Bold => "<font-weight=\"700\">" + text + "</font-weight>", 
			TextFontWeight.Heavy => "<font-weight=\"800\">" + text + "</font-weight>", 
			TextFontWeight.Black => "<font-weight=\"900\">" + text + "</font-weight>", 
			_ => text, 
		};
	}

	public static string ApplyStyle(this string text, Kingmaker.Code.View.Bridge.Data.TextStyle style)
	{
		return "<style=" + style.Style.name + ">" + text + "</style>";
	}

	public static string GetBookFormat(string text, TMP_FontAsset font, Color color = default(Color), int size = 140, float voffset = 0f, Material fontMaterial = null)
	{
		string text2 = text.Trim();
		if (string.IsNullOrEmpty(text) || font == null)
		{
			return text;
		}
		int i;
		for (i = 0; i < text2.Length && !IsLetterOrOpeningTag(text2[i]); i++)
		{
		}
		if (i >= text2.Length)
		{
			return text;
		}
		char c2 = text2[i];
		if (c2 == '<')
		{
			int j;
			for (j = text2.IndexOf('>', i); j < text2.Length && !char.IsLetter(text2[j]); j++)
			{
			}
			if (j < text2.Length)
			{
				c2 = text2[j];
				i = j;
			}
		}
		string text3 = ColorUtility.ToHtmlStringRGB((color != default(Color)) ? color : ConfigRoot.Instance.UIConfig.PaperSaberColor);
		string text4 = text2.Substring(0, i);
		string text5 = ((fontMaterial != null) ? (" material=\"" + fontMaterial.name + "\"") : string.Empty);
		object[] obj = new object[8] { text4, voffset, text3, size, font.name, text5, c2, null };
		string text6 = text2;
		int num = i + 1;
		obj[7] = text6.Substring(num, text6.Length - num);
		return string.Format("{0}<voffset={1}em><color=#{2}><size={3}%><font=\"{4}\"{5}>{6}</font></size></color></voffset>{7}", obj);
		static bool IsLetterOrOpeningTag(char c)
		{
			if (!char.IsLetter(c))
			{
				return c == '<';
			}
			return true;
		}
	}

	public static Sprite GetIconByText(string text)
	{
		Sprite[] abilityPlaceholderIcon = ConfigRoot.Instance.UIConfig.UIIcons.AbilityPlaceholderIcon;
		if (text == null)
		{
			return abilityPlaceholderIcon[0];
		}
		int num = text.Length - abilityPlaceholderIcon.Length * Convert.ToInt32(Math.Floor(Convert.ToDecimal(text.Length / abilityPlaceholderIcon.Length)));
		return abilityPlaceholderIcon[num];
	}

	public static Color32 GetColorByText(string text)
	{
		Color32[] randomColors = ConfigRoot.Instance.UIConfig.RandomColors;
		if (text == null)
		{
			return randomColors[0];
		}
		int num = text.Length - randomColors.Length * Convert.ToInt32(Math.Floor(Convert.ToDecimal(text.Length / randomColors.Length)));
		return randomColors[num];
	}

	public static string ArabicToRoman(int number)
	{
		return UtilityText.ArabicToRoman(number);
	}

	public static string StringIDToColor(string source, string stringID, Color color)
	{
		return UtilityText.StringIDToColor(source, stringID, color);
	}

	public static string GetTextByKey(DamageType typeDescriptionEnergy)
	{
		return UtilityText.GetDamageTypeText(typeDescriptionEnergy);
	}

	public static void TryAddWordSeparator(StringBuilder description, string conjunction)
	{
		UtilityText.TryAddWordSeparator(description, conjunction);
	}

	public static string AddSign(float? value)
	{
		return UtilityText.AddSign(value);
	}

	public static string FormatModifier(int value, ModifierType modifierType)
	{
		char prefix = modifierType.GetPrefix();
		char suffix = modifierType.GetSuffix();
		return $"{prefix}{value}{suffix}";
	}

	public static char GetPrefix(this ModifierType type)
	{
		return type switch
		{
			ModifierType.PctAdd => '+', 
			ModifierType.ValAdd => '+', 
			ModifierType.ValAdd_Extra => '+', 
			ModifierType.PctMul => '×', 
			ModifierType.PctMul_Extra => '×', 
			_ => '?', 
		};
	}

	public static char GetSuffix(this ModifierType type)
	{
		return type switch
		{
			ModifierType.PctAdd => '%', 
			ModifierType.PctMul => '%', 
			ModifierType.PctMul_Extra => '%', 
			ModifierType.ValAdd => '\0', 
			ModifierType.ValAdd_Extra => '\0', 
			_ => '?', 
		};
	}
}
