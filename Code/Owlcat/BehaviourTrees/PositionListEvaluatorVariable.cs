using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Owlcat.AI;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class PositionListEvaluatorVariable : PositionListVariable
{
	private readonly PositionListEvaluator m_Evaluator;

	private List<Vector3> m_LastEvaluatedValue;

	public override List<Vector3> Value
	{
		get
		{
			try
			{
				m_LastEvaluatedValue = m_Evaluator.GetValue();
			}
			catch (FailToEvaluateException)
			{
				m_LastEvaluatedValue = null;
			}
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of PositionListEvaluatorVariable");
		}
	}

	public PositionListEvaluatorVariable(PositionListEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
