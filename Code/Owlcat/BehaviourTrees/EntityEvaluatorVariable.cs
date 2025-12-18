using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;

namespace Owlcat.BehaviourTrees;

public class EntityEvaluatorVariable : EntityVariable
{
	private readonly MechanicEntityEvaluator m_Evaluator;

	private MechanicEntity m_LastEvaluatedValue;

	public override MechanicEntity Value
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
			throw new Exception("Can't set value of EntityEvaluatorVariable");
		}
	}

	public EntityEvaluatorVariable(MechanicEntityEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
