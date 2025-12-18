using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickCombatLogBaseVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly int NestedLevel;

	public readonly bool IsResultValue;

	public readonly string ResultValue;

	public readonly bool IsProtectionIcon;

	public readonly bool IsTargetHitIcon;

	public readonly bool IsBorderChanceIcon;

	public readonly bool IsGrayBackground;

	public readonly bool IsBeigeBackground;

	public readonly bool IsRedBackground;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickCombatLogBaseVM(string name, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground, TooltipBaseTemplate tooltip)
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
