using System;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("10c8c11acea64f0b8b1daa0b91d92d68")]
public class ContextConditionIsInCover : ContextCondition
{
	public bool UseBestShootingPosition;

	public LosCalculations.CoverType MinimalCover;

	protected override string GetConditionCaption()
	{
		return "Check if target is in cover";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
