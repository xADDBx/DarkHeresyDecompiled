using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.View.Bridge.Services;

public class BrickDamageRangeArgs : BrickCombatLogBaseArgs
{
	public int CurrentValue { get; }

	public int MinValue { get; }

	public int MaxValue { get; }

	public BrickDamageRangeArgs(string name, int currentValue, int minValue, int maxValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon icon = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal)
		: base(name, nestedLevel, isResultValue, resultValue, icon, palette)
	{
		CurrentValue = currentValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}
}
