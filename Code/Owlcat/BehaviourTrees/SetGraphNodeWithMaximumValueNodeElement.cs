using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode with Maximum Value", "Set GraphNode with Maximum Value")]
[TypeId("3d54cd5ce23d4a8b9c4d1fb00d7a4df3")]
public class SetGraphNodeWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetGraphNodeWithMaximumValueNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeListVariableReference NodesList;

	public MechanicEntityListVariableReference Entities;

	public PropertyCalculator FunctionToMaximize;

	protected override SetGraphNodeWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodesList.GetRuntimeVariable(blackboard);
		MechanicEntityListVariable runtimeVariable3 = Entities.GetRuntimeVariable(blackboard);
		return new SetGraphNodeWithMaximumValueNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, FunctionToMaximize);
	}
}
