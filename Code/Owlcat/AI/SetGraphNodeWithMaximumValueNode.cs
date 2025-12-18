using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Owlcat.AI.Mechanics.Positioning;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeWithMaximumValueNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeVariable m_Variable;

	private readonly GraphNodeListVariable m_NodesList;

	private readonly MechanicEntityListVariable m_Entities;

	private readonly PropertyCalculator m_FunctionToMaximize;

	public SetGraphNodeWithMaximumValueNode(EntityVariable agent, GraphNodeVariable variable, GraphNodeListVariable nodes, MechanicEntityListVariable entities, PropertyCalculator functionToMaximize)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_NodesList = nodes;
		m_Entities = entities;
		m_FunctionToMaximize = functionToMaximize;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetGraphNodeWithMaximumValue(m_Agent.Value, m_Entities.Value, m_NodesList.Value, m_FunctionToMaximize);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static GraphNode GetGraphNodeWithMaximumValue(MechanicEntity agent, List<MechanicEntity> entities, List<GraphNode> nodes, PropertyCalculator utilityFunction)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return null;
		}
		if (utilityFunction == null || utilityFunction.Empty)
		{
			return nodes[0];
		}
		GraphNode result = null;
		int num = int.MinValue;
		List<MechanicEntity> list = (((entities?.Count ?? 0) > 0) ? entities : new List<MechanicEntity> { agent });
		using (ContextData<AiPositioningData>.Request().Setup(agent.SizeRect))
		{
			foreach (GridNodeBase item in nodes.OfType<GridNodeBase>())
			{
				int num2 = 0;
				using (ContextData<AiPositioningData>.Request().Setup(item))
				{
					foreach (MechanicEntity item2 in list)
					{
						num2 += utilityFunction.GetValue(new PropertyContext(agent, null, item2));
					}
					if (num2 > num)
					{
						num = num2;
						result = item;
					}
				}
			}
			return result;
		}
	}
}
