namespace Owlcat.BehaviourTrees;

public class IntegerConditionPassNode : ConditionPassNode
{
	private readonly IntegerVariable m_LeftVariable;

	private readonly IntegerVariable m_RightVariable;

	private readonly IntegerCompareOperator m_CompareOperator;

	public IntegerConditionPassNode(AbortType abortType, IntegerVariable leftVariable, IntegerVariable rightVariable, IntegerCompareOperator compareOperator)
		: base(abortType)
	{
		m_LeftVariable = leftVariable;
		m_RightVariable = rightVariable;
		m_CompareOperator = compareOperator;
	}

	public override bool IsPassed()
	{
		return IntegerConditionNode.IsPassed(m_LeftVariable, m_RightVariable, m_CompareOperator);
	}
}
