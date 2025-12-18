using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Flow Control/Squad Control", "Squad Control")]
[TypeId("7efe83d4a0974b8890540078aba27a85")]
public class SquadControlNodeElement : BehaviourTreeNodeElement<SquadControlNode>
{
	protected override SquadControlNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable = blackboard.GetRuntimeInternalDataVariable();
		return new SquadControlNode(agentVariable, runtimeInternalDataVariable);
	}
}
