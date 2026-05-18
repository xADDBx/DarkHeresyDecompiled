using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BrickCombatLogBaseVM : TooltipBrickVM
{
	public readonly string Name;

	public readonly int NestedLevel;

	public readonly bool IsResultValue;

	public readonly string ResultValue;

	public readonly CombatLogIcon IconType;

	public readonly BrickElementPalette Palette;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickCombatLogBaseVM(string name, int nestedLevel, bool isResultValue, string resultValue, CombatLogIcon iconType, BrickElementPalette palette, TooltipBaseTemplate tooltip)
	{
		Name = name;
		NestedLevel = nestedLevel;
		IsResultValue = isResultValue;
		ResultValue = resultValue;
		IconType = iconType;
		Palette = palette;
		Tooltip = tooltip;
	}
}
