using System;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("02cb972126ad12049ba2da7ed67c4eb5")]
public class DayTime : Condition
{
	public TimeOfDay Time;

	protected override string GetConditionCaption()
	{
		return $"Day time is {Time}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.TimeOfDay == Time;
	}
}
