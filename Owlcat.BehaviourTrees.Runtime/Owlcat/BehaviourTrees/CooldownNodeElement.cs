using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Cooldown", "Cooldown")]
[TypeId("7a65c972a57743bbaac8748b205e325f")]
public class CooldownNodeElement : BehaviourTreeNodeElement<CooldownNode>
{
	public FloatVariableReference CooldownVariable;

	public WhenBlockPassRule WhenBlockPassRule;

	public ResultInBlockedStateRule ResultInBlockedStateRule = ResultInBlockedStateRule.Failure;

	protected override CooldownNode CreateTypedNode(Blackboard blackboard)
	{
		return new CooldownNode(CooldownVariable.GetRuntimeVariable(blackboard), WhenBlockPassRule, ResultInBlockedStateRule);
	}
}
