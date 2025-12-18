using System.Collections.Generic;
using Kingmaker.UnitLogic;

namespace Kingmaker.Code.View.Bridge.Services;

public readonly struct TooltipBrickTriggeredAutoArgs
{
	public string TriggeredAutoText { get; }

	public IReadOnlyList<FeatureCountableFlag.FactsList.Element> ReasonItems { get; }

	public bool IsSuccess { get; }

	public TooltipBrickTriggeredAutoArgs(string triggeredAutoText, IReadOnlyList<FeatureCountableFlag.FactsList.Element> reasonItems, bool isSuccess)
	{
		TriggeredAutoText = triggeredAutoText;
		ReasonItems = reasonItems;
		IsSuccess = isSuccess;
	}
}
