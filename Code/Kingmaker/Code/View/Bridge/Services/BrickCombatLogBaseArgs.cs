using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public abstract class BrickCombatLogBaseArgs
{
	public string Name { get; }

	public int NestedLevel { get; }

	public bool IsResultValue { get; }

	public string ResultValue { get; }

	public CombatLogIcon IconType { get; }

	public BrickElementPalette Palette { get; }

	public TooltipBaseTemplate Tooltip { get; }

	protected BrickCombatLogBaseArgs(string name, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon iconType = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal, TooltipBaseTemplate tooltip = null)
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
