using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNodeList/Set GraphNodeList with Filter", "Set GraphNodeList with Filter")]
[TypeId("597da39e60594467948c19eb27f7f362")]
public class SetGraphNodeListWithFilterNodeElement : BehaviourTreeNodeElement<SetGraphNodeListWithFilterNode>
{
	public GraphNodeListVariableReference Variable;

	public GraphNodeListVariableReference NodeList;

	public MechanicEntityListVariableReference Entities;

	public PropertyCalculator Filter;

	protected override SetGraphNodeListWithFilterNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodeList.GetRuntimeVariable(blackboard);
		MechanicEntityListVariable runtimeVariable3 = Entities.GetRuntimeVariable(blackboard);
		return new SetGraphNodeListWithFilterNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, Filter);
	}
}
