using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Owlcat.AI.Mechanics.Positioning;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeListWithFilterNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeListVariable m_Variable;

	private readonly GraphNodeListVariable m_NodeList;

	private readonly MechanicEntityListVariable m_Entities;

	private readonly PropertyCalculator m_Filter;

	public SetGraphNodeListWithFilterNode(EntityVariable agent, GraphNodeListVariable variable, GraphNodeListVariable nodeList, MechanicEntityListVariable entities, PropertyCalculator filter)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_NodeList = nodeList;
		m_Entities = entities;
		m_Filter = filter;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetFilteredNodes(m_Agent.Value, m_Entities.Value, m_NodeList.Value, m_Filter);
		if (m_Variable.Value.Count <= 0)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static List<GraphNode> GetFilteredNodes(MechanicEntity agent, List<MechanicEntity> entities, List<GraphNode> nodes, PropertyCalculator filter)
	{
		List<GraphNode> list = new List<GraphNode>();
		if (filter == null || filter.Empty)
		{
			list.AddRange(nodes);
			return list;
		}
		List<MechanicEntity> list2 = (((entities?.Count ?? 0) > 0) ? entities : new List<MechanicEntity> { agent });
		using (ContextData<AiPositioningData>.Request().Setup(agent.SizeRect))
		{
			foreach (GridNodeBase item in nodes.OfType<GridNodeBase>())
			{
				using (ContextData<AiPositioningData>.Request().Setup(item))
				{
					foreach (MechanicEntity item2 in list2)
					{
						if (filter.GetBoolValue(new PropertyContext(agent, null, item2)))
						{
							list.Add(item);
						}
					}
				}
			}
			return list;
		}
	}
}
