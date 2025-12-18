using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/End Turn", "End Turn")]
[TypeId("5e61773480384d3690481b0f51ce473e")]
public class EndTurnNodeElement : BehaviourTreeNodeElement<EndTurnNode>
{
	protected override EndTurnNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new EndTurnNode(agentVariable, runtimeInternalDataVariable);
	}
}
