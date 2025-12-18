using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Conditions/Mechanic Condition Pass Node", "Mechanic Condition Pass Node")]
[TypeId("3584f6c518e543b5ab32bd5b4dd3f671")]
public class MechanicConditionPassNodeElement : ConditionPassNodeElement<MechanicConditionPassNode>
{
	public ConditionsChecker ConditionsChecker;

	protected override MechanicConditionPassNode CreateTypedNode(Blackboard blackboard)
	{
		return new MechanicConditionPassNode(AbortType, ConditionsChecker);
	}
}
