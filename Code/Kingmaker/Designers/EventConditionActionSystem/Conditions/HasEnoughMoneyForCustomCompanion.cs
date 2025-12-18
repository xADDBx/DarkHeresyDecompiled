using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("7e2fe92f8f828b64dacbc8384bf9061b")]
public class HasEnoughMoneyForCustomCompanion : Condition
{
	protected override string GetConditionCaption()
	{
		return "Has enough money for custom companion";
	}

	protected override bool CheckCondition()
	{
		return true;
	}
}
