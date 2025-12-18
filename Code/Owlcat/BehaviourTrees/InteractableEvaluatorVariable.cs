using System;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;

namespace Owlcat.BehaviourTrees;

public class InteractableEvaluatorVariable : InteractableVariable
{
	private readonly InteractionActionEvaluator m_Evaluator;

	private InteractionAction m_LastEvaluatedValue;

	public override InteractionAction Value
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
			throw new Exception("Can't set value of InteractableEvaluatorVariable");
		}
	}

	public InteractableEvaluatorVariable(InteractionActionEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
