using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("4f09301d9ac14ab499bcf9aea330e25b")]
public class ContextConditionHasPF : ContextCondition
{
	public float Value;

	protected override string GetConditionCaption()
	{
		return $"Check if has at least {Value} total PF";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
