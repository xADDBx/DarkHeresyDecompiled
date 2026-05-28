using System;

namespace Owlcat.BehaviourTrees;

public class IntegerConditionNode : ConditionNode
{
	private readonly IntegerVariable m_LeftVariable;

	private readonly IntegerVariable m_RightVariable;

	private readonly IntegerCompareOperator m_CompareOperator;

	public IntegerConditionNode(AbortType abortType, IntegerVariable leftVariable, IntegerVariable rightVariable, IntegerCompareOperator compareOperator)
		: base(abortType)
	{
		m_LeftVariable = leftVariable;
		m_RightVariable = rightVariable;
		m_CompareOperator = compareOperator;
	}

	public override bool IsPassed()
	{
		return IsPassed(m_LeftVariable, m_RightVariable, m_CompareOperator);
	}

	public static bool IsPassed(IntegerVariable leftVariable, IntegerVariable rightVariable, IntegerCompareOperator compareOperator)
	{
		return compareOperator switch
		{
			IntegerCompareOperator.Equal => leftVariable.Value == rightVariable.Value, 
			IntegerCompareOperator.NotEqual => leftVariable.Value != rightVariable.Value, 
			IntegerCompareOperator.Greater => leftVariable.Value > rightVariable.Value, 
			IntegerCompareOperator.GreaterOrEqual => leftVariable.Value >= rightVariable.Value, 
			IntegerCompareOperator.Less => leftVariable.Value < rightVariable.Value, 
			IntegerCompareOperator.LessOrEqual => leftVariable.Value <= rightVariable.Value, 
			_ => throw new Exception($"Not supported compare operator: {compareOperator}"), 
		};
	}
}
