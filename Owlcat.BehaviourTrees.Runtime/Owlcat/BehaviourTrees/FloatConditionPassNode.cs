namespace Owlcat.BehaviourTrees;

public class FloatConditionPassNode : ConditionPassNode
{
	private readonly FloatVariable m_LeftVariable;

	private readonly FloatVariable m_RightVariable;

	private readonly FloatCompareOperator m_CompareOperator;

	public FloatConditionPassNode(AbortType abortType, FloatVariable leftVariable, FloatVariable rightVariable, FloatCompareOperator compareOperator)
		: base(abortType)
	{
		m_LeftVariable = leftVariable;
		m_RightVariable = rightVariable;
		m_CompareOperator = compareOperator;
	}

	public override bool IsPassed()
	{
		return FloatConditionNode.IsPassed(m_LeftVariable, m_RightVariable, m_CompareOperator);
	}
}
