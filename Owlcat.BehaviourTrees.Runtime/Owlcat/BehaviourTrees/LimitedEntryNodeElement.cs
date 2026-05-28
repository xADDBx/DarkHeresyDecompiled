using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Limited Entry", "Limited Entry")]
[TypeId("ae1d2cbdf76a47de9f44e464dc030196")]
public class LimitedEntryNodeElement : BehaviourTreeNodeElement<LimitedEntryNode>
{
	public IntegerVariableReference EntryLimitVariableReference;

	public WhenBlockPassRule WhenBlockPassRule;

	public ResultInBlockedStateRule ResultInBlockedStateRule = ResultInBlockedStateRule.Failure;

	protected override LimitedEntryNode CreateTypedNode(Blackboard blackboard)
	{
		return new LimitedEntryNode(EntryLimitVariableReference.GetRuntimeVariable(blackboard), WhenBlockPassRule, ResultInBlockedStateRule);
	}
}
