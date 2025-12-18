using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickDamageRangeVM : TooltipBrickCombatLogBaseVM
{
	public readonly int CurrentValue;

	public readonly int MinValue;

	public readonly int MaxValue;

	public TooltipBrickDamageRangeVM(string name, int currentValue, int minValue, int maxValue, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground, tooltip)
	{
		CurrentValue = currentValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}
}
