using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChanceVM : BrickCombatLogBaseVM
{
	public readonly int SufficientValue;

	public readonly int? CurrentValue;

	public BrickChanceVM(string name, int sufficientValue, int? currentValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon icon = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal)
		: base(name, nestedLevel, isResultValue, resultValue, icon, palette, null)
	{
		SufficientValue = sufficientValue;
		CurrentValue = currentValue;
	}
}
