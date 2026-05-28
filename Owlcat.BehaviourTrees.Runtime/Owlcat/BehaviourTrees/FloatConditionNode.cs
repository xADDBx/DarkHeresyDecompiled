using System;

namespace Owlcat.BehaviourTrees;

public class FloatConditionNode : ConditionNode
{
	private readonly FloatVariable m_LeftVariable;

	private readonly FloatVariable m_RightVariable;

	private readonly FloatCompareOperator m_CompareOperator;

	public FloatConditionNode(AbortType abortType, FloatVariable leftVariable, FloatVariable rightVariable, FloatCompareOperator compareOperator)
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

	public static bool IsPassed(FloatVariable leftVariable, FloatVariable rightVariable, FloatCompareOperator compareOperator)
	{
		return compareOperator switch
		{
			FloatCompareOperator.Greater => leftVariable.Value > rightVariable.Value, 
			FloatCompareOperator.Less => leftVariable.Value < rightVariable.Value, 
			_ => throw new Exception($"Not supported compare operator: {compareOperator}"), 
		};
	}
}
