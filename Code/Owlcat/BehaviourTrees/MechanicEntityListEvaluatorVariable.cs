using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.AI;

namespace Owlcat.BehaviourTrees;

public class MechanicEntityListEvaluatorVariable : MechanicEntityListVariable
{
	private readonly MechanicEntityListEvaluator m_Evaluator;

	private List<MechanicEntity> m_LastEvaluatedValue;

	public override List<MechanicEntity> Value
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
			throw new Exception("Can't set value of MechanicEntityListEvaluatorVariable");
		}
	}

	public MechanicEntityListEvaluatorVariable(MechanicEntityListEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
