using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("0436e1194e6a558438138ceca9172e5f")]
public class HasEnoughMoneyForRespec : Condition
{
	protected override string GetConditionCaption()
	{
		return "Has enough money for respec";
	}

	protected override bool CheckCondition()
	{
		return true;
	}
}
