namespace Kingmaker.Code.View.Bridge.Services;

public class TooltipBrickChanceArgs : TooltipBrickCombatLogBaseArgs
{
	public int SufficientValue { get; }

	public int? CurrentValue { get; }

	public TooltipBrickChanceArgs(string name, int sufficientValue, int? currentValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		SufficientValue = sufficientValue;
		CurrentValue = currentValue;
	}
}
