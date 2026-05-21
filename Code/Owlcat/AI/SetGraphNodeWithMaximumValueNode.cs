using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility;
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

	[CanBeNull]
	private readonly PropertyCalculatorBlueprintVariable m_CalculatorBlueprint;

	private bool HasScoringFunction
	{
		get
		{
			if (m_CalculatorBlueprint?.Value == null)
			{
				if (m_FunctionToMaximize != null)
				{
					return !m_FunctionToMaximize.Empty;
				}
				return false;
			}
			return true;
		}
	}

	public SetGraphNodeWithMaximumValueNode(EntityVariable agent, GraphNodeVariable variable, GraphNodeListVariable nodes, MechanicEntityListVariable entities, PropertyCalculator functionToMaximize, [CanBeNull] PropertyCalculatorBlueprintVariable calculatorBlueprint)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_NodesList = nodes;
		m_Entities = entities;
		m_FunctionToMaximize = functionToMaximize;
		m_CalculatorBlueprint = calculatorBlueprint;
	}

	private int EvaluateScore(MechanicEntity entity, TargetWrapper target = null)
	{
		PropertyCalculatorBlueprint propertyCalculatorBlueprint = m_CalculatorBlueprint?.Value;
		if (propertyCalculatorBlueprint != null)
		{
			return propertyCalculatorBlueprint.Value.GetValue(entity, null, target, null, null, PropertyCalculator.ExceptionHandlingMode.ThrowImmediately) + propertyCalculatorBlueprint.Add;
		}
		return m_FunctionToMaximize.GetValue(entity, null, target);
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetGraphNodeWithMaximumValue(m_Agent.Value, m_Entities.Value, m_NodesList.Value);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private GraphNode GetGraphNodeWithMaximumValue(MechanicEntity agent, List<MechanicEntity> entities, List<GraphNode> nodes)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return null;
		}
		if (!HasScoringFunction)
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
						num2 += EvaluateScore(agent, item2);
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
