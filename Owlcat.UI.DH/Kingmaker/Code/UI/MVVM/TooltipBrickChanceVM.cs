using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickChanceVM : TooltipBrickCombatLogBaseVM
{
	public readonly int SufficientValue;

	public readonly int? CurrentValue;

	public TooltipBrickChanceVM(string name, int sufficientValue, int? currentValue, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground, TooltipBaseTemplate tooltip = null)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground, tooltip)
	{
		SufficientValue = sufficientValue;
		CurrentValue = currentValue;
	}
}
