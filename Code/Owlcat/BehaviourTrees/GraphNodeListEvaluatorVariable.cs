using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Owlcat.AI;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

public class GraphNodeListEvaluatorVariable : GraphNodeListVariable
{
	private readonly GraphNodeListEvaluator m_Evaluator;

	private List<GraphNode> m_LastEvaluatedValue;

	public override List<GraphNode> Value
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
			throw new Exception("Can't set value of GraphNodeListEvaluatorVariable");
		}
	}

	public GraphNodeListEvaluatorVariable(GraphNodeListEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
