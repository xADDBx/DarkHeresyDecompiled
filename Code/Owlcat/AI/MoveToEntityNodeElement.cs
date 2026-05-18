using Kingmaker.UnitLogic.Squads.Goals;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Move/Move to Entity", "Move to Entity")]
[TypeId("5f4766043c0343739c67680f65284a8d")]
public class MoveToEntityNodeElement : BehaviourTreeNodeElement<MoveToEntityNode>
{
	public EntityVariableReference TargetEntity;

	public AiThreatsHandlingStrategy ThreatsHandlingStrategy;

	public PreferredPositionNearEntity PreferredPositionNearEntity;

	public SquadGoalKind SquadGoalKind;

	protected override MoveToEntityNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		EntityVariable runtimeVariable = TargetEntity.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new MoveToEntityNode(agentVariable, runtimeVariable, runtimeInternalDataVariable, ThreatsHandlingStrategy, PreferredPositionNearEntity, SquadGoalKind);
	}
}
