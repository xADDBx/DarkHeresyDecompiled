using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Move/Move to GraphNode", "Move to GraphNode")]
[TypeId("aeb92deb4b4048f4ab8b7a365fda6872")]
public class MoveToGraphNodeNodeElement : BehaviourTreeNodeElement<MoveToGraphNodeNode>
{
	public GraphNodeVariableReference TargetNode;

	public TargetNodeSelectionPolicy TargetNodeSelectionPolicy;

	public AiThreatsHandlingStrategy ThreatsHandlingStrategy;

	protected override MoveToGraphNodeNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		GraphNodeVariable runtimeVariable = TargetNode.GetRuntimeVariable(blackboard);
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new MoveToGraphNodeNode(agentVariable, runtimeVariable, runtimeInternalDataVariable, TargetNodeSelectionPolicy, ThreatsHandlingStrategy);
	}
}
