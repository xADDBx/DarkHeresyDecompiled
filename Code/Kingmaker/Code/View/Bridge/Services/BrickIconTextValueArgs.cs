using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public class BrickIconTextValueArgs : BrickCombatLogBaseArgs
{
	public string Value { get; }

	public BrickIconTextValueArgs(string name, string value, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon iconType = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, iconType, palette, tooltip)
	{
		Value = value;
	}
}
