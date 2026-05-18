using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SetMechanicEntityListNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly MechanicEntityListVariable m_Variable;

	private readonly MechanicEntityListVariable m_Entities;

	private readonly PropertyCalculator m_Filter;

	public SetMechanicEntityListNode(EntityVariable agent, MechanicEntityListVariable variable, MechanicEntityListVariable entities, PropertyCalculator filter)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Entities = entities;
		m_Filter = filter;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetFilteredEntities(m_Agent.Value, m_Entities.Value, m_Filter);
		if (m_Variable.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<MechanicEntity> GetFilteredEntities(MechanicEntity agent, List<MechanicEntity> entities, PropertyCalculator filter)
	{
		List<MechanicEntity> list = new List<MechanicEntity>();
		if (entities == null || entities.Count == 0)
		{
			return list;
		}
		if (filter == null || filter.Empty)
		{
			list.AddRange(entities);
			return list;
		}
		foreach (MechanicEntity entity in entities)
		{
			if (filter.GetBoolValue(agent, null, entity))
			{
				list.Add(entity);
			}
		}
		return list;
	}
}
