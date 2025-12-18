using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public abstract class TooltipBrickCombatLogBaseArgs
{
	public string Name { get; }

	public int NestedLevel { get; }

	public bool IsResultValue { get; }

	public string ResultValue { get; }

	public bool IsProtectionIcon { get; }

	public bool IsTargetHitIcon { get; }

	public bool IsBorderChanceIcon { get; }

	public bool IsGrayBackground { get; }

	public bool IsBeigeBackground { get; }

	public bool IsRedBackground { get; }

	public TooltipBaseTemplate Tooltip { get; }

	protected TooltipBrickCombatLogBaseArgs(string name, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false, TooltipBaseTemplate tooltip = null)
	{
		Name = name;
		NestedLevel = nestedLevel;
		IsResultValue = isResultValue;
		ResultValue = resultValue;
		IsProtectionIcon = isProtectionIcon;
		IsTargetHitIcon = isTargetHitIcon;
		IsBorderChanceIcon = isBorderChanceIcon;
		IsGrayBackground = isGrayBackground;
		IsBeigeBackground = isBeigeBackground;
		IsRedBackground = isRedBackground;
		Tooltip = tooltip;
	}
}
