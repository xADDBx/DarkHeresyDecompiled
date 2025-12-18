using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode to Use Ability From", "Set GraphNode to Use Ability From")]
[TypeId("115532e9ba5b4651a7a0e24f5ef6a58f")]
public class SetGraphNodeToUseAbilityFromNodeElement : BehaviourTreeNodeElement<SetGraphNodeToUseAbilityFromNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeListVariableReference NodesList;

	public MechanicEntityListVariableReference Targets;

	public AbilityVariableReference Ability;

	public PropertyCalculator FunctionToMaximize;

	protected override SetGraphNodeToUseAbilityFromNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable2 = NodesList.GetRuntimeVariable(blackboard);
		MechanicEntityListVariable runtimeVariable3 = Targets.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable4 = Ability.GetRuntimeVariable(blackboard);
		return new SetGraphNodeToUseAbilityFromNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, runtimeVariable4, FunctionToMaximize);
	}
}
