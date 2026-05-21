using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.View.Covers;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class SetGraphNodeToUseAoeAbilityToNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeVariable m_Variable;

	private readonly GraphNodeVariable m_CasterNode;

	private readonly GraphNodeListVariable m_NodesList;

	private readonly AbilityVariable m_Ability;

	private readonly int m_MinTotalValueToCastAbility;

	private readonly PropertyCalculator m_TargetValueCalculator;

	private readonly bool m_IncludeDeadUnitsInCalculations;

	[CanBeNull]
	private readonly PropertyCalculatorBlueprintVariable m_CalculatorBlueprint;

	public SetGraphNodeToUseAoeAbilityToNode(EntityVariable agent, GraphNodeVariable variable, GraphNodeVariable casterNode, GraphNodeListVariable nodes, AbilityVariable ability, int minTotalValueToCastAbility, PropertyCalculator targetValueCalculator, bool includeDeadUnitsInCalculations, [CanBeNull] PropertyCalculatorBlueprintVariable calculatorBlueprint = null)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_CasterNode = casterNode;
		m_NodesList = nodes;
		m_Ability = ability;
		m_MinTotalValueToCastAbility = minTotalValueToCastAbility;
		m_TargetValueCalculator = targetValueCalculator;
		m_IncludeDeadUnitsInCalculations = includeDeadUnitsInCalculations;
		m_CalculatorBlueprint = calculatorBlueprint;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetGraphNodeForUseAoeAbilityTo(m_Agent.Value, (GridNodeBase)m_CasterNode.Value, m_NodesList.Value, m_Ability.Value, m_MinTotalValueToCastAbility, m_TargetValueCalculator, m_IncludeDeadUnitsInCalculations, out var _, m_CalculatorBlueprint?.Value);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	internal static GraphNode GetGraphNodeForUseAoeAbilityTo(MechanicEntity caster, GridNodeBase casterNode, List<GraphNode> nodes, AbilityData ability, int minValueToCastAbility, PropertyCalculator utilityFunction, bool includeDeadUnitsInCalculations, out int maxScore, PropertyCalculatorBlueprint calculatorBlueprint = null)
	{
		maxScore = int.MinValue;
		if (nodes == null || nodes.Count == 0 || ability == null || !ability.CanTargetPoint)
		{
			return null;
		}
		if (casterNode == null)
		{
			return null;
		}
		GraphNode result = null;
		HashSet<MechanicEntity> coveredTargets = new HashSet<MechanicEntity>();
		foreach (GridNodeBase item in nodes.OfType<GridNodeBase>())
		{
			if (!CanTargetFromNode(ability, casterNode, item))
			{
				continue;
			}
			if (calculatorBlueprint == null && (utilityFunction == null || utilityFunction.Empty))
			{
				return item;
			}
			coveredTargets.Clear();
			GatherAffectedTargets(ability, casterNode, item, in coveredTargets);
			int num = 0;
			foreach (MechanicEntity item2 in coveredTargets)
			{
				if (!item2.IsDeadOrUnconscious || includeDeadUnitsInCalculations)
				{
					num += ((calculatorBlueprint != null) ? (calculatorBlueprint.Value.GetValue(caster, null, item2, null, ability, PropertyCalculator.ExceptionHandlingMode.ThrowImmediately) + calculatorBlueprint.Add) : utilityFunction.GetValue(caster, null, item2, null, ability, PropertyCalculator.ExceptionHandlingMode.ThrowImmediately));
				}
			}
			if (num > maxScore)
			{
				maxScore = num;
				result = item;
			}
		}
		if (maxScore < minValueToCastAbility)
		{
			return null;
		}
		return result;
	}

	private static void GatherAffectedTargets(AbilityData ability, GridNodeBase casterNode, GridNodeBase targetNode, in HashSet<MechanicEntity> coveredTargets)
	{
		IAbilityAoEPatternProvider patternSettings = ability.GetPatternSettings();
		if (patternSettings == null)
		{
			return;
		}
		foreach (GridNodeBase node in patternSettings.GetOrientedPattern(ability, casterNode, targetNode, Size.Medium, coveredTargetsOnly: true).Nodes)
		{
			if (node.TryGetFirstUnit(out var unit))
			{
				coveredTargets.Add(unit);
			}
		}
	}

	private static bool CanTargetFromNode(AbilityData ability, GridNodeBase casterNode, GridNodeBase targetNode)
	{
		int distance;
		LosCalculations.CoverType los;
		return ability.CanTargetFromNode(casterNode, targetNode, targetNode.Vector3Position(), out distance, out los);
	}
}
