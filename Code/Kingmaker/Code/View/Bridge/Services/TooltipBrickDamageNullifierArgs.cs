using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.Code.View.Bridge.Services;

public readonly struct TooltipBrickDamageNullifierArgs
{
	public int ChanceRoll { get; }

	public int ResultRoll { get; }

	public int ResultValue { get; }

	public string ReasonText { get; }

	public IReadOnlyList<BuffInformation> ReasonItems { get; }

	public string ResultText { get; }

	public TooltipBrickDamageNullifierArgs(int chanceRoll, int resultRoll, int resultValue, string reasonText, IReadOnlyList<BuffInformation> reasonItems, string resultText)
	{
		ChanceRoll = chanceRoll;
		ResultRoll = resultRoll;
		ResultValue = resultValue;
		ReasonText = reasonText;
		ReasonItems = reasonItems;
		ResultText = resultText;
	}
}
