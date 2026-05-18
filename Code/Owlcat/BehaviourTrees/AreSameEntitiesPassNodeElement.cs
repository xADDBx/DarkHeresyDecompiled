using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/MechanicEntity/Are Same Entities Pass Node", "Are Same Entities Pass Node")]
[TypeId("24451c6345e0481f9fb525fe22cb42b2")]
public class AreSameEntitiesPassNodeElement : ConditionPassNodeElement<AreSameEntitiesPassNode>
{
	public EntityVariableReference Entity1;

	public EntityVariableReference Entity2;

	public bool Invert;

	protected override AreSameEntitiesPassNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = Entity1.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Entity2.GetRuntimeVariable(blackboard);
		return new AreSameEntitiesPassNode(AbortType, runtimeVariable, runtimeVariable2, Invert);
	}
}
