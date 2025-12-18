using System;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class PositionEvaluatorVariable : PositionVariable
{
	private readonly PositionEvaluator m_Evaluator;

	private Vector3 m_LastEvaluatedValue;

	public override Vector3 Value
	{
		get
		{
			m_LastEvaluatedValue = m_Evaluator.GetValue();
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of PositionEvaluatorVariable");
		}
	}

	public PositionEvaluatorVariable(PositionEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ({2})", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
