using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.UI.MVVM;

public class BrickDamageRangeVM : BrickCombatLogBaseVM
{
	public readonly int CurrentValue;

	public readonly int MinValue;

	public readonly int MaxValue;

	public BrickDamageRangeVM(string name, int currentValue, int minValue, int maxValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, CombatLogIcon iconType = CombatLogIcon.None, BrickElementPalette palette = BrickElementPalette.Normal)
		: base(name, nestedLevel, isResultValue, resultValue, iconType, palette, null)
	{
		CurrentValue = currentValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}
}
