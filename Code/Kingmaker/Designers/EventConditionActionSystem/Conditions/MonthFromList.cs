using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("dc6c9a45489fdab408c8269d8457f699")]
public class MonthFromList : Condition
{
	public int[] Months;

	protected override string GetConditionCaption()
	{
		return $"Month number is in list";
	}

	protected override bool CheckCondition()
	{
		return Months.Contains((ConfigRoot.Instance.Calendar.GetStartDate() + Game.Instance.Controllers.TimeController.GameTime).Month);
	}
}
