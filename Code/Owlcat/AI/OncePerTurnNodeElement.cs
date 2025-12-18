using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Flow Control/Once Per Turn", "Once Per Turn")]
[TypeId("0a2f71194313427f8e2c7cc503d1c0cc")]
public class OncePerTurnNodeElement : BehaviourTreeNodeElement<OncePerTurnNode>
{
	public WhenBlockPassRule WhenBlockPassRule;

	public ResultInBlockedStateRule ResultInBlockedStateRule = ResultInBlockedStateRule.Failure;

	protected override OncePerTurnNode CreateTypedNode(Blackboard blackboard)
	{
		return new OncePerTurnNode(blackboard.GetRuntimeInternalDataVariable(), WhenBlockPassRule, ResultInBlockedStateRule);
	}
}
