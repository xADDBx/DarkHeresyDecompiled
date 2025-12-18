using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("978c68cedc69f104bab225e02a47b69a")]
public class DayOfTheMonth : Condition
{
	public int Day;

	protected override string GetConditionCaption()
	{
		return $"Day of month is {Day}";
	}

	protected override bool CheckCondition()
	{
		return (ConfigRoot.Instance.Calendar.GetStartDate() + Game.Instance.Controllers.TimeController.GameTime).Day == Day;
	}
}
