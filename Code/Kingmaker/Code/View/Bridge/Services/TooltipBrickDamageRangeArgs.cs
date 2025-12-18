namespace Kingmaker.Code.View.Bridge.Services;

public class TooltipBrickDamageRangeArgs : TooltipBrickCombatLogBaseArgs
{
	public int CurrentValue { get; }

	public int MinValue { get; }

	public int MaxValue { get; }

	public TooltipBrickDamageRangeArgs(string name, int currentValue, int minValue, int maxValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		CurrentValue = currentValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}
}
