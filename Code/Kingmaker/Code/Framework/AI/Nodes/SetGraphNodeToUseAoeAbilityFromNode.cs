using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Kingmaker.Code.Framework.AI.Nodes;

public class SetGraphNodeToUseAoeAbilityFromNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeVariable m_AoEVariable;

	private readonly GraphNodeVariable m_PositionVariable;

	private readonly GraphNodeListVariable m_CasterNodeList;

	private readonly GraphNodeListVariable m_NodesList;

	private readonly AbilityVariable m_Ability;

	private readonly int m_MinTotalValueToCastAbility;

	private readonly PropertyCalculator m_TargetValueCalculator;

	private readonly bool m_IncludeDeadUnitsInCalculations;

	[CanBeNull]
	private readonly PropertyCalculatorBlueprintVariable m_CalculatorBlueprint;

	public SetGraphNodeToUseAoeAbilityFromNode(EntityVariable agent, GraphNodeVariable aoeVariable, GraphNodeVariable positionVariable, GraphNodeListVariable casterNodes, GraphNodeListVariable nodes, AbilityVariable ability, int minTotalValueToCastAbility, PropertyCalculator targetValueCalculator, bool includeDeadUnitsInCalculations, [CanBeNull] PropertyCalculatorBlueprintVariable calculatorBlueprint = null)
	{
		m_Agent = agent;
		m_AoEVariable = aoeVariable;
		m_PositionVariable = positionVariable;
		m_CasterNodeList = casterNodes;
		m_NodesList = nodes;
		m_Ability = ability;
		m_MinTotalValueToCastAbility = minTotalValueToCastAbility;
		m_TargetValueCalculator = targetValueCalculator;
		m_IncludeDeadUnitsInCalculations = includeDeadUnitsInCalculations;
		m_CalculatorBlueprint = calculatorBlueprint;
	}

	public override NodeVisitResult ForwardVisit()
	{
		GraphNode resultAoeGraphNode;
		GraphNode resultPositionNode;
		bool graphNodeForUseAoeAbilityTo = GetGraphNodeForUseAoeAbilityTo(m_Agent.Value, m_CasterNodeList.Value, m_NodesList.Value, m_Ability.Value, m_MinTotalValueToCastAbility, m_TargetValueCalculator, m_IncludeDeadUnitsInCalculations, out resultAoeGraphNode, out resultPositionNode, m_CalculatorBlueprint?.Value);
		m_AoEVariable.Value = resultAoeGraphNode;
		m_PositionVariable.Value = resultPositionNode;
		if (!graphNodeForUseAoeAbilityTo)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static bool GetGraphNodeForUseAoeAbilityTo(MechanicEntity caster, List<GraphNode> casterNodeList, List<GraphNode> aoeNodes, AbilityData ability, int minValueToCastAbility, PropertyCalculator utilityFunction, bool includeDeadUnitsInCalculations, out GraphNode resultAoeGraphNode, out GraphNode resultPositionNode, PropertyCalculatorBlueprint calculatorBlueprint = null)
	{
		int num = int.MinValue;
		resultAoeGraphNode = null;
		resultPositionNode = null;
		if (casterNodeList == null || casterNodeList.Count == 0)
		{
			return false;
		}
		foreach (GraphNode casterNode in casterNodeList)
		{
			int maxScore;
			GraphNode graphNodeForUseAoeAbilityTo = SetGraphNodeToUseAoeAbilityToNode.GetGraphNodeForUseAoeAbilityTo(caster, (GridNodeBase)casterNode, aoeNodes, ability, minValueToCastAbility, utilityFunction, includeDeadUnitsInCalculations, out maxScore, calculatorBlueprint);
			if (maxScore > num)
			{
				resultAoeGraphNode = graphNodeForUseAoeAbilityTo;
				resultPositionNode = casterNode;
				num = maxScore;
			}
		}
		if (aoeNodes == null || aoeNodes.Count == 0 || ability == null || !ability.CanTargetPoint)
		{
			return false;
		}
		return resultAoeGraphNode != null;
	}
}
