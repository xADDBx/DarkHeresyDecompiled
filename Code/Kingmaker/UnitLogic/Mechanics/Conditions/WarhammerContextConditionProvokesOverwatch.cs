using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("07ccf425d1ce6694b9a15eac6e39462c")]
public class WarhammerContextConditionProvokesOverwatch : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check if ability provokes overwatch";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
