using System;
using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class BooleanEvaluatorVariable : BooleanVariable
{
	private readonly BooleanEvaluator m_Evaluator;

	private bool m_LastEvaluatedValue;

	public override bool Value
	{
		get
		{
			m_LastEvaluatedValue = m_Evaluator.GetValue();
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of BooleanEvaluatorVariable");
		}
	}

	public BooleanEvaluatorVariable(BooleanEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
