using System;
using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class IntegerEvaluatorVariable : IntegerVariable
{
	private readonly IntEvaluator m_Evaluator;

	private int m_LastEvaluatedValue;

	public override int Value
	{
		get
		{
			m_LastEvaluatedValue = m_Evaluator.GetValue();
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of IntegerEvaluatorVariable");
		}
	}

	public IntegerEvaluatorVariable(IntEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ({2})", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
