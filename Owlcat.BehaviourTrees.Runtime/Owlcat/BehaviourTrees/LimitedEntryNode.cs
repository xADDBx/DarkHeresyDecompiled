namespace Owlcat.BehaviourTrees;

public class LimitedEntryNode : BlockPassNode
{
	private readonly IntegerVariable m_EntryLimitVariable;

	public int EntryCount { get; private set; }

	public LimitedEntryNode(IntegerVariable entryLimit, WhenBlockPassRule whenBlockPassRule, ResultInBlockedStateRule resultInBlockedStateRule)
		: base(whenBlockPassRule, resultInBlockedStateRule)
	{
		m_EntryLimitVariable = entryLimit;
	}

	public override bool IsStillBlocked()
	{
		return EntryCount >= m_EntryLimitVariable.Value;
	}

	protected override void Block()
	{
		EntryCount++;
	}
}
