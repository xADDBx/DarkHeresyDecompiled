using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconTextValueVM : BrickCombatLogBaseVM
{
	public readonly string Value;

	public BrickIconTextValueVM(string name, string value, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon icon = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, icon, palette, tooltip)
	{
		Value = value;
	}
}
