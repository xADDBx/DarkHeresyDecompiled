using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Entity/Set Entity", "Set Entity")]
[TypeId("743fdf44c6bf43bba1dbedcce5f63f88")]
public class SetEntityNodeElement : BehaviourTreeNodeElement<SetEntityNode>
{
	public EntityVariableReference Variable;

	public EntityVariableReference Value;

	protected override SetEntityNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Value.GetRuntimeVariable(blackboard);
		return new SetEntityNode(runtimeVariable, runtimeVariable2);
	}
}
