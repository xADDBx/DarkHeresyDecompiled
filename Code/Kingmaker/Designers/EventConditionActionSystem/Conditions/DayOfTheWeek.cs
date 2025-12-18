using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("291e56cd48cc34d4daae4a46e1aa1886")]
public class DayOfTheWeek : Condition
{
	public DayOfWeek Day;

	protected override string GetConditionCaption()
	{
		return $"Day of week is {Day}";
	}

	protected override bool CheckCondition()
	{
		return (ConfigRoot.Instance.Calendar.GetStartDate() + Game.Instance.Controllers.TimeController.GameTime).DayOfWeek == Day;
	}
}
