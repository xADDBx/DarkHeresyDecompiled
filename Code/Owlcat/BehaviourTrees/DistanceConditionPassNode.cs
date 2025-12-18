using System;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class DistanceConditionPassNode : ConditionPassNode
{
	private readonly EntityVariable m_FromVariable;

	private readonly EntityVariable m_ToVariable;

	private readonly FloatVariable m_DistanceVariable;

	private readonly FloatCompareOperator m_CompareOperator;

	public DistanceConditionPassNode(AbortType abortType, EntityVariable fromVariable, EntityVariable toVariable, FloatVariable distanceVariable, FloatCompareOperator compareOperator)
		: base(abortType)
	{
		m_FromVariable = fromVariable;
		m_ToVariable = toVariable;
		m_DistanceVariable = distanceVariable;
		m_CompareOperator = compareOperator;
	}

	public override bool IsPassed()
	{
		return m_CompareOperator switch
		{
			FloatCompareOperator.Greater => Vector3.Distance(m_FromVariable.Value.Position, m_ToVariable.Value.Position) > m_DistanceVariable.Value, 
			FloatCompareOperator.Less => Vector3.Distance(m_FromVariable.Value.Position, m_ToVariable.Value.Position) < m_DistanceVariable.Value, 
			_ => throw new Exception($"Not supported compare operator: {m_CompareOperator}"), 
		};
	}
}
