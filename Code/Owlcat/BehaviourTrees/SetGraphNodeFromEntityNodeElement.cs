using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode from Entity", "Set GraphNode from Entity")]
[TypeId("30e857319b94420a926e9918c47378ce")]
public class SetGraphNodeFromEntityNodeElement : BehaviourTreeNodeElement<SetGraphNodeFromEntityNode>
{
	public GraphNodeVariableReference Variable;

	public EntityVariableReference Entity;

	protected override SetGraphNodeFromEntityNode CreateTypedNode(Blackboard blackboard)
	{
		GraphNodeVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Entity.GetRuntimeVariable(blackboard);
		return new SetGraphNodeFromEntityNode(runtimeVariable, runtimeVariable2);
	}
}
