using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Owlcat.AI.Mechanics.Positioning;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeToUseAbilityFromNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeVariable m_Variable;

	private readonly GraphNodeListVariable m_NodesList;

	private readonly MechanicEntityListVariable m_Targets;

	private readonly AbilityVariable m_Ability;

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

	public SetGraphNodeToUseAbilityFromNode(EntityVariable agent, GraphNodeVariable variable, GraphNodeListVariable nodes, MechanicEntityListVariable targets, AbilityVariable ability, PropertyCalculator functionToMaximize, [CanBeNull] PropertyCalculatorBlueprintVariable calculatorBlueprint = null)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_NodesList = nodes;
		m_Targets = targets;
		m_Ability = ability;
		m_FunctionToMaximize = functionToMaximize;
		m_CalculatorBlueprint = calculatorBlueprint;
	}

	private int EvaluateScore(MechanicEntity entity, TargetWrapper target = null, AbilityData ability = null)
	{
		PropertyCalculatorBlueprint propertyCalculatorBlueprint = m_CalculatorBlueprint?.Value;
		if (propertyCalculatorBlueprint != null)
		{
			return propertyCalculatorBlueprint.Value.GetValue(entity, null, target, null, ability, PropertyCalculator.ExceptionHandlingMode.ThrowImmediately) + propertyCalculatorBlueprint.Add;
		}
		return m_FunctionToMaximize.GetValue(entity, null, target, null, ability);
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetGraphNodeToUseAbilityFrom(m_Agent.Value, m_Targets.Value, m_NodesList.Value, m_Ability.Value);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private GraphNode GetGraphNodeToUseAbilityFrom(MechanicEntity caster, List<MechanicEntity> targets, List<GraphNode> nodes, AbilityData ability)
	{
		if (nodes == null || nodes.Count == 0 || ability == null || ability.Blueprint == null)
		{
			return null;
		}
		if (targets == null || targets.Count == 0)
		{
			return null;
		}
		GraphNode result = null;
		int num = int.MinValue;
		using (ContextData<AiPositioningData>.Request().Setup(caster.SizeRect))
		{
			foreach (GridNodeBase item in nodes.OfType<GridNodeBase>())
			{
				if (!CanTargetAnyoneFromNode(ability, item, targets))
				{
					continue;
				}
				if (!HasScoringFunction)
				{
					return item;
				}
				int num2 = 0;
				using (ContextData<AiPositioningData>.Request().Setup(item))
				{
					foreach (MechanicEntity target in targets)
					{
						num2 += EvaluateScore(caster, target, ability);
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

	private static bool CanTargetAnyoneFromNode(AbilityData ability, GridNodeBase node, List<MechanicEntity> targets)
	{
		int distance;
		LosCalculations.CoverType los;
		return targets.Any((MechanicEntity t) => ability.CanTargetFromNode(node, null, t, out distance, out los));
	}
}
