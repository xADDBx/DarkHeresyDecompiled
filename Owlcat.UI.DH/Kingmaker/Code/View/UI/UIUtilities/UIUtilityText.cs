using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps.Simple;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityText
{
	public static readonly string ForceTextSymbol = "<sprite name=\"Force\">";

	public static UITooltips UITooltips => LocalizedTexts.Instance.UserInterfacesText.Tooltips;

	public static string GetCostFormatted(float value)
	{
		return value.ToString("0.#");
	}

	public static string GetLongOrShortText(string description, bool state)
	{
		string text = description;
		if (state)
		{
			if (text.Contains("[LONGSTART]"))
			{
				while (text.Contains("[LONGSTART]"))
				{
					int startIndex = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
					int startIndex2 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal) + 11;
					string text2 = text.Substring(startIndex2);
					text = text.Remove(startIndex) + text2;
				}
			}
			if (text.Contains("[LONGEND]"))
			{
				while (text.Contains("[LONGEND]"))
				{
					int startIndex3 = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
					int startIndex4 = text.IndexOf("[LONGEND]", StringComparison.Ordinal) + 9;
					string text3 = text.Substring(startIndex4);
					text = text.Remove(startIndex3) + text3;
				}
			}
		}
		else if (text.Contains("[LONGSTART]") && text.Contains("[LONGEND]"))
		{
			while (text.Contains("[LONGSTART]"))
			{
				int startIndex5 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
				int num = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
				string text4 = text.Remove(startIndex5);
				if (num < 0)
				{
					text = text4;
					continue;
				}
				int num2 = num + 9;
				string text5 = text;
				int num3 = num2;
				string text6 = text5.Substring(num3, text5.Length - num3);
				text = text4 + text6;
			}
		}
		return text;
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

	public static string UpdateDescriptionWithUIProperties(string description, MechanicEntity calculationSource)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			int num = 0;
			string text = string.Empty;
			try
			{
				while (num < description.Length)
				{
					int num2 = description.IndexOf("{uip|", num, StringComparison.InvariantCulture);
					if (num2 == -1)
					{
						text += description.Substring(num);
						break;
					}
					_ = description[num2];
					text += description.Substring(num, num2 - num);
					num = num2;
					num2 += 5;
					int num3 = description.Substring(num2).IndexOf('|');
					int num4 = description.Substring(num2).IndexOf('}');
					if (num3 == -1 || num4 == -1 || num3 > num4)
					{
						return description;
					}
					num2 += num3;
					string link = description.Substring(num + 5, num2 - num - 5);
					num = num2 + 1;
					num2 += description.Substring(num2).IndexOf('}');
					string assetId = description.Substring(num, num2 - num);
					num = num2 + 1;
					BlueprintMechanicEntityFact blueprintMechanicEntityFact = ResourcesLibrary.TryGetBlueprint<BlueprintMechanicEntityFact>(assetId);
					if (calculationSource != null)
					{
						MechanicEntityFact mechanicEntityFact = GetMechanicEntityFact(calculationSource, blueprintMechanicEntityFact);
						UIPropertiesComponent uIPropertiesComponent = mechanicEntityFact?.GetComponent<UIPropertiesComponent>();
						if (uIPropertiesComponent == null)
						{
							uIPropertiesComponent = blueprintMechanicEntityFact?.GetComponent<UIPropertiesComponent>();
						}
						UIPropertySettings property = uIPropertiesComponent?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
						if (property != null)
						{
							int? num5 = (property.PropertySource ?? blueprintMechanicEntityFact).GetComponents<IPropertyCalculatorComponent>().FirstOrDefault((IPropertyCalculatorComponent c) => c.Name == property.PropertyName)?.GetValue(mechanicEntityFact?.Context, calculationSource);
							string glossaryMechanicsHTML = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
							if (num5.HasValue)
							{
								link = $"<b><color={glossaryMechanicsHTML}><link=\"uip:{blueprintMechanicEntityFact.AssetGuid}:{link}:{calculationSource.UniqueId}\">{Mathf.Abs(num5.Value)}</link></color></b>";
							}
						}
						else
						{
							property = blueprintMechanicEntityFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == link);
							if (property != null)
							{
								link = property.Description;
							}
						}
					}
					else
					{
						UIPropertySettings uIPropertySettings = blueprintMechanicEntityFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
						if (uIPropertySettings != null)
						{
							link = uIPropertySettings.Description;
						}
					}
					text += link;
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"{arg}");
			}
			return text;
		}
	}

	private static MechanicEntityFact GetMechanicEntityFact(MechanicEntity mechanicEntity, BlueprintMechanicEntityFact blueprintFact)
	{
		MechanicEntityFact mechanicEntityFact = null;
		mechanicEntityFact = mechanicEntity.Facts.Get<Ability>(blueprintFact);
		if (mechanicEntityFact == null)
		{
			mechanicEntityFact = mechanicEntity.Facts.Get<ToggleAbility>(blueprintFact);
		}
		if (mechanicEntityFact == null)
		{
			mechanicEntityFact = mechanicEntity.Facts.Get<UnitFact>(blueprintFact);
		}
		return mechanicEntityFact;
	}

	public static string UpdateDescriptionWithUICommonProperties(string description, MechanicEntity calculationSource)
	{
		int num = 0;
		string text = string.Empty;
		while (num < description.Length)
		{
			int num2 = description.IndexOf("{uicp|", num, StringComparison.InvariantCulture);
			if (num2 == -1)
			{
				text += description.Substring(num);
				break;
			}
			_ = description[num2];
			text += description.Substring(num, num2 - num);
			num = num2;
			num2 += description.Substring(num2).IndexOf('}');
			string link = description.Substring(num + 6, num2 - num - 6);
			ContextProperty link2 = StatTypeFromString(link);
			num = num2 + 1;
			link = GetCommonPropertyStringFromStatType(link2, calculationSource as UnitEntity);
			text += link;
		}
		return text;
	}

	private static ContextProperty StatTypeFromString(string link)
	{
		return link switch
		{
			"BallisticSkill" => ContextProperty.BallisticSkill, 
			"WeaponSkill" => ContextProperty.WeaponSkill, 
			"Strength" => ContextProperty.Strength, 
			"Toughness" => ContextProperty.Toughness, 
			"Agility" => ContextProperty.Agility, 
			"Intelligence" => ContextProperty.Intelligence, 
			"Willpower" => ContextProperty.Willpower, 
			"Perception" => ContextProperty.Perception, 
			"Fellowship" => ContextProperty.Fellowship, 
			"BallisticSkillBonus" => ContextProperty.BallisticSkillBonus, 
			"WeaponSkillBonus" => ContextProperty.WeaponSkillBonus, 
			"StrengthBonus" => ContextProperty.StrengthBonus, 
			"ToughnessBonus" => ContextProperty.ToughnessBonus, 
			"AgilityBonus" => ContextProperty.AgilityBonus, 
			"IntelligenceBonus" => ContextProperty.IntelligenceBonus, 
			"WillpowerBonus" => ContextProperty.WillpowerBonus, 
			"PerceptionBonus" => ContextProperty.PerceptionBonus, 
			"FellowshipBonus" => ContextProperty.FellowshipBonus, 
			"Resolve" => ContextProperty.Resolve, 
			_ => ContextProperty.None, 
		};
	}

	private static string GetCommonPropertyStringFromStatType(ContextProperty link, UnitEntity caster)
	{
		if (caster != null)
		{
			return link switch
			{
				ContextProperty.BallisticSkill => "{g|Encyclopedia:WarhammerBallisticSkill}" + caster.Stats.GetStat(StatType.BallisticSkill)?.ToString() + "{/g}", 
				ContextProperty.WeaponSkill => "{g|Encyclopedia:WarhammerWeaponSkill}" + caster.Stats.GetStat(StatType.WeaponSkill)?.ToString() + "{/g}", 
				ContextProperty.Strength => "{g|Encyclopedia:WarhammerStrength}" + caster.Stats.GetStat(StatType.Strength)?.ToString() + "{/g}", 
				ContextProperty.Toughness => "{g|Encyclopedia:WarhammerToughness}" + caster.Stats.GetStat(StatType.Toughness)?.ToString() + "{/g}", 
				ContextProperty.Agility => "{g|Encyclopedia:WarhammerAgility}" + caster.Stats.GetStat(StatType.Agility)?.ToString() + "{/g}", 
				ContextProperty.Intelligence => "{g|Encyclopedia:WarhammerIntelligence}" + caster.Stats.GetStat(StatType.Intelligence)?.ToString() + "{/g}", 
				ContextProperty.Willpower => "{g|Encyclopedia:WarhammerWillpower}" + caster.Stats.GetStat(StatType.Willpower)?.ToString() + "{/g}", 
				ContextProperty.Perception => "{g|Encyclopedia:WarhammerPerception}" + caster.Stats.GetStat(StatType.Perception)?.ToString() + "{/g}", 
				ContextProperty.Fellowship => "{g|Encyclopedia:WarhammerFellowship}" + caster.Stats.GetStat(StatType.Fellowship)?.ToString() + "{/g}", 
				ContextProperty.BallisticSkillBonus => "{g|Encyclopedia:WarhammerBallisticSkill}" + (caster.Stats.GetAttributeOptional(StatType.BallisticSkill)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.WeaponSkillBonus => "{g|Encyclopedia:WarhammerWeaponSkill}" + (caster.Stats.GetAttributeOptional(StatType.WeaponSkill)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.StrengthBonus => "{g|Encyclopedia:WarhammerStrength}" + (caster.Stats.GetAttributeOptional(StatType.Strength)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.ToughnessBonus => "{g|Encyclopedia:WarhammerToughness}" + (caster.Stats.GetAttributeOptional(StatType.Toughness)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.AgilityBonus => "{g|Encyclopedia:WarhammerAgility}" + (caster.Stats.GetAttributeOptional(StatType.Agility)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.IntelligenceBonus => "{g|Encyclopedia:WarhammerIntelligence}" + (caster.Stats.GetAttributeOptional(StatType.Intelligence)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.WillpowerBonus => "{g|Encyclopedia:WarhammerWillpower}" + (caster.Stats.GetAttributeOptional(StatType.Willpower)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.PerceptionBonus => "{g|Encyclopedia:WarhammerPerception}" + (caster.Stats.GetAttributeOptional(StatType.Perception)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.FellowshipBonus => "{g|Encyclopedia:WarhammerFellowship}" + (caster.Stats.GetAttributeOptional(StatType.Fellowship)?.Bonus ?? 0) + "{/g}", 
				ContextProperty.Resolve => "<u>{g|Encyclopedia:Resolve}resolve" + caster.Stats.GetStat(StatType.Resolve)?.ToString() + "{/g}</u>", 
				_ => "UNKNOWN STAT", 
			};
		}
		return link switch
		{
			ContextProperty.BallisticSkill => "{g|Encyclopedia:WarhammerBallisticSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.BallisticSkill) + "{/g}", 
			ContextProperty.WeaponSkill => "{g|Encyclopedia:WarhammerWeaponSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WeaponSkill) + "{/g}", 
			ContextProperty.Strength => "{g|Encyclopedia:WarhammerStrength}" + LocalizedTexts.Instance.Stats.GetText(StatType.Strength) + "{/g}", 
			ContextProperty.Toughness => "{g|Encyclopedia:WarhammerToughness}" + LocalizedTexts.Instance.Stats.GetText(StatType.Toughness) + "{/g}", 
			ContextProperty.Agility => "{g|Encyclopedia:WarhammerAgility}" + LocalizedTexts.Instance.Stats.GetText(StatType.Agility) + "{/g}", 
			ContextProperty.Intelligence => "{g|Encyclopedia:WarhammerIntelligence}" + LocalizedTexts.Instance.Stats.GetText(StatType.Intelligence) + "{/g}", 
			ContextProperty.Willpower => "{g|Encyclopedia:WarhammerWillpower}" + LocalizedTexts.Instance.Stats.GetText(StatType.Willpower) + "{/g}", 
			ContextProperty.Perception => "{g|Encyclopedia:WarhammerPerception}" + LocalizedTexts.Instance.Stats.GetText(StatType.Perception) + "{/g}", 
			ContextProperty.Fellowship => "{g|Encyclopedia:WarhammerFellowship}" + LocalizedTexts.Instance.Stats.GetText(StatType.Fellowship) + "{/g}", 
			ContextProperty.BallisticSkillBonus => "{g|Encyclopedia:WarhammerBallisticSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.BallisticSkill) + "{/g}", 
			ContextProperty.WeaponSkillBonus => "{g|Encyclopedia:WarhammerWeaponSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WeaponSkill) + "{/g}", 
			ContextProperty.StrengthBonus => "{g|Encyclopedia:WarhammerStrength}" + LocalizedTexts.Instance.Stats.GetText(StatType.Strength) + "{/g}", 
			ContextProperty.ToughnessBonus => "{g|Encyclopedia:WarhammerToughness}" + LocalizedTexts.Instance.Stats.GetText(StatType.Toughness) + "{/g}", 
			ContextProperty.AgilityBonus => "{g|Encyclopedia:WarhammerAgility}" + LocalizedTexts.Instance.Stats.GetText(StatType.Agility) + "{/g}", 
			ContextProperty.IntelligenceBonus => "{g|Encyclopedia:WarhammerIntelligence}" + LocalizedTexts.Instance.Stats.GetText(StatType.Intelligence) + "{/g}", 
			ContextProperty.WillpowerBonus => "{g|Encyclopedia:WarhammerWillpower}" + LocalizedTexts.Instance.Stats.GetText(StatType.Willpower) + "{/g}", 
			ContextProperty.PerceptionBonus => "{g|Encyclopedia:WarhammerPerception}" + LocalizedTexts.Instance.Stats.GetText(StatType.Perception) + "{/g}", 
			ContextProperty.FellowshipBonus => "{g|Encyclopedia:WarhammerFellowship}" + LocalizedTexts.Instance.Stats.GetText(StatType.Fellowship) + "{/g}", 
			ContextProperty.Resolve => "<u>{g|Encyclopedia:Resolve}resolve" + LocalizedTexts.Instance.Stats.GetText(StatType.Resolve) + "{/g}</u>", 
			_ => "UNKNOWN STAT", 
		};
	}

	public static string GetPercentString(float value)
	{
		if (!(value >= 0f))
		{
			return "";
		}
		return $"{Mathf.Round(value)}%";
	}

	public static string GetModifierString(int value)
	{
		if (value <= 0)
		{
			return value.ToString();
		}
		return "+" + value;
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

	public static bool IsNullOrInvisible(string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static LocalizedString GetSoulMarkRankText(int index)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		if (index > 0)
		{
			return index switch
			{
				1 => alignment.AlignmentMarkRankTier1, 
				2 => alignment.AlignmentMarkRankTier2, 
				3 => alignment.AlignmentMarkRankTier3, 
				4 => alignment.AlignmentMarkRankTier4, 
				5 => alignment.AlignmentMarkRankTier5, 
				_ => new LocalizedString(), 
			};
		}
		return alignment.SoulMarkRankTierNone;
	}

	public static LocalizedString GetSoulMarkDirectionText(AlignmentAxis direction)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		return direction switch
		{
			AlignmentAxis.Xenophilia => alignment.Xenophilia, 
			AlignmentAxis.Xanthite => alignment.Xanthite, 
			AlignmentAxis.Monodominance => alignment.Monodominance, 
			AlignmentAxis.Torian => alignment.Torian, 
			AlignmentAxis.None => new LocalizedString(), 
			_ => new LocalizedString(), 
		};
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
