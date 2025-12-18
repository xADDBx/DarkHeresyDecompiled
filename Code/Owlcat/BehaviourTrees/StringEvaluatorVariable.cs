using System;
using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class StringEvaluatorVariable : StringVariable
{
	private readonly StringEvaluator m_Evaluator;

	private string m_LastEvaluatedValue;

	public override string Value
	{
		get
		{
			m_LastEvaluatedValue = m_Evaluator.GetValue();
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of StringEvaluatorVariable");
		}
	}

	public StringEvaluatorVariable(StringEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return base.Key + ": " + m_LastEvaluatedValue + " (" + ((m_Evaluator == null) ? "null" : m_Evaluator.GetCaption()) + ")";
	}
}
