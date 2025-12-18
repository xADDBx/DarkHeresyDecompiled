using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public class TooltipBrickIconTextValueArgs : TooltipBrickCombatLogBaseArgs
{
	public string Value { get; }

	public TooltipBrickIconTextValueArgs(string name, string value, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground, tooltip)
	{
		Value = value;
	}
}
