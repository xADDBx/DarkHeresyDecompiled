using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/MechanicEntity/Are Same Entities Leaf", "Are Same Entities")]
[TypeId("f614b4b71fb048aa9d1230b8fc1154aa")]
public class AreSameEntitiesNodeElement : ConditionNodeElement<AreSameEntitiesNode>
{
	public EntityVariableReference Entity1;

	public EntityVariableReference Entity2;

	public bool Invert;

	protected override AreSameEntitiesNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = Entity1.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Entity2.GetRuntimeVariable(blackboard);
		return new AreSameEntitiesNode(AbortType, runtimeVariable, runtimeVariable2, Invert);
	}
}
