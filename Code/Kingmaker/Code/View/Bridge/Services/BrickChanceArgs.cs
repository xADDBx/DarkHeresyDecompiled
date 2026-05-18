using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.View.Bridge.Services;

public class BrickChanceArgs : BrickCombatLogBaseArgs
{
	public int SufficientValue { get; }

	public int? CurrentValue { get; }

	public BrickChanceArgs(string name, int sufficientValue, int? currentValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon icon = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal)
		: base(name, nestedLevel, isResultValue, resultValue, icon, palette)
	{
		SufficientValue = sufficientValue;
		CurrentValue = currentValue;
	}
}
