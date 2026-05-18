using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("18b4005ab45ded44dbacab84cab0b247")]
public class ContextConditionIsMainTarget : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Is main target";
	}

	protected override bool CheckCondition()
	{
		return base.Eval.ClickedTarget?.Equals(base.Target) ?? false;
	}
}
