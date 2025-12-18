using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Mechanic Condition Leaf", "Mechanic Condition Leaf")]
[TypeId("432381c99d3a4b5b89ce42f5b46ee4d0")]
public class MechanicConditionNodeElement : ConditionNodeElement<MechanicConditionNode>
{
	public ConditionsChecker ConditionsChecker;

	protected override MechanicConditionNode CreateTypedNode(Blackboard blackboard)
	{
		return new MechanicConditionNode(AbortType, ConditionsChecker);
	}
}
