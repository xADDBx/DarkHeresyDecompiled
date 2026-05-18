using System;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityText
{
	public static string ArabicToRoman(int number)
	{
		if (number == 0)
		{
			return "-";
		}
		StringBuilder stringBuilder = new StringBuilder();
		int[] array = new int[13]
		{
			1, 4, 5, 9, 10, 40, 50, 90, 100, 400,
			500, 900, 1000
		};
		string[] array2 = new string[13]
		{
			"I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD",
			"D", "CM", "M"
		};
		while (number > 0)
		{
			for (int num = array.Length - 1; num >= 0; num--)
			{
				if (number / array[num] >= 1)
				{
					number -= array[num];
					stringBuilder.Append(array2[num]);
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string StringIDToColor(string source, string stringID, Color color)
	{
		return source?.Replace(stringID, ColorUtility.ToHtmlStringRGB(color));
	}

	public static string GetDamageTypeText(DamageType typeDescriptionEnergy)
	{
		LocalizedString localizedString = ConfigRoot.Instance.LocalizedTexts.DamageTypes.Entries.FirstOrDefault((DamageTypeStrings.MyEntry entry) => entry.Value == typeDescriptionEnergy)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	[Obsolete]
	public static string GetConditionText(UnitCondition condition)
	{
		LocalizedString localizedString = ConfigRoot.Instance.LocalizedTexts.UnitConditions.Entries.FirstOrDefault((ConditionsString.MyEntry entry) => entry.Value == condition)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	public static void TryAddWordSeparator(StringBuilder description, string conjunction)
	{
		if (description.Length > 0)
		{
			description.Append(" " + conjunction + " ");
		}
	}

	public static string FormatModifierValue(float? value, ModifierType modifierType)
	{
		if (!value.HasValue)
		{
			return "–";
		}
		if (modifierType == ModifierType.PctMul || modifierType == ModifierType.PctMul_Extra)
		{
			return (UtilityMath.ToFraction(value.Value) + 1f).ToString("0:00");
		}
		return AddSign(value);
	}

	public static string AddSign(float? value)
	{
		if (!value.HasValue)
		{
			return "–";
		}
		if (!(value >= 0f))
		{
			return "–" + (0f - value);
		}
		float? num = value;
		return "+" + num;
	}

	public static string ToStringWithSign(this int number)
	{
		if (number < 0)
		{
			return "–" + -number;
		}
		return "+" + number;
	}
}
