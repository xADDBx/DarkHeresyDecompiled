using System;
using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class FloatEvaluatorVariable : FloatVariable
{
	private readonly FloatEvaluator m_Evaluator;

	private float m_LastEvaluatedValue;

	public override float Value
	{
		get
		{
			m_LastEvaluatedValue = m_Evaluator.GetValue();
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of FloatEvaluatorVariable");
		}
	}

	public FloatEvaluatorVariable(FloatEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ({2})", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
