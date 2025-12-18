using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/GraphNode/Set GraphNode to Use AoE Ability To", "Set GraphNode to Use AoE Ability To")]
[TypeId("35c43790034c4da59c78d078820f99eb")]
public class SetGraphNodeToUseAoeAbilityToNodeElement : BehaviourTreeNodeElement<SetGraphNodeToUseAoeAbilityToNode>
{
	public GraphNodeVariableReference Variable;

	public GraphNodeVariableReference CasterNode;

	public GraphNodeListVariableReference NodesList;

	public AbilityVariableReference Ability;

	public int MinTotalValueToCastAbility;

	public PropertyCalculator TargetValueCalculator;

	public bool IncludeDeadUnitsInCalculations;

	protected override SetGraphNodeToUseAoeAbilityToNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		GraphNodeVariable runtimeVariable2 = CasterNode.GetRuntimeVariable(blackboard);
		GraphNodeListVariable runtimeVariable3 = NodesList.GetRuntimeVariable(blackboard);
		AbilityVariable runtimeVariable4 = Ability.GetRuntimeVariable(blackboard);
		return new SetGraphNodeToUseAoeAbilityToNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, runtimeVariable4, MinTotalValueToCastAbility, TargetValueCalculator, IncludeDeadUnitsInCalculations);
	}
}
