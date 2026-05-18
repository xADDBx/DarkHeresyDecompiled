using System.Globalization;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Code.Framework.Utility;

public static class FormattedStringExtension
{
	public static string GetFormattedName(this Modifier modifier)
	{
		string bonusSourceText = StatModifiersBreakdown.GetBonusSourceText(modifier);
		if (string.IsNullOrEmpty(bonusSourceText))
		{
			return LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
		}
		return bonusSourceText;
	}

	public static string GetFormattedValue(this Modifier modifier)
	{
		ModifierFormatFlags modifierFormatFlags = ModifierFormatFlags.None;
		if (modifier.IsPercent)
		{
			modifierFormatFlags |= ModifierFormatFlags.IsPercent;
		}
		return modifier.GetFormattedValue(string.Empty, modifierFormatFlags);
	}

	public static string GetFormattedValue(this Modifier modifier, string suffix, ModifierFormatFlags formatFlags)
	{
		bool num = formatFlags.HasFlag(ModifierFormatFlags.IsPercent);
		bool num2 = formatFlags.HasFlag(ModifierFormatFlags.NoPlusSign);
		string text = "";
		string text2 = ((modifier.Type == ModifierType.PctMul) ? "×" : "");
		string text3 = ((!num2 && modifier.Value > 0 && modifier.Type != ModifierType.PctMul && modifier.Type != ModifierType.PctMul_Extra) ? "+" : "");
		string text4 = (((num && modifier.Type == ModifierType.ValAdd) || modifier.Type == ModifierType.PctAdd) ? "%" : "");
		float num3 = modifier.Value;
		if (num && modifier.Type == ModifierType.PctAdd)
		{
			num3 = num3 / 100f + 1f;
			text2 = "×";
			text3 = "";
			text4 = "";
		}
		ModifierType type = modifier.Type;
		if (type == ModifierType.PctMul || type == ModifierType.PctMul_Extra)
		{
			text2 = "×";
			num3 /= 100f;
		}
		if (!text2.IsNullOrEmpty())
		{
			text = " (" + (num3 * 100f).ToString(CultureInfo.InvariantCulture) + "%)";
		}
		return text2 + text3 + num3.ToString(CultureInfo.InvariantCulture) + text4 + suffix + text;
	}
}
