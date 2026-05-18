using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Owlcat.BehaviourTrees;

public class SetEntityWithMaximumValueNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly EntityVariable m_Variable;

	private readonly MechanicEntityListVariable m_Entities;

	[CanBeNull]
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

	public SetEntityWithMaximumValueNode(EntityVariable agent, EntityVariable variable, MechanicEntityListVariable entities, [CanBeNull] AbilityVariable ability, PropertyCalculator functionToMaximize, [CanBeNull] PropertyCalculatorBlueprintVariable calculatorBlueprint)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Entities = entities;
		m_Ability = ability;
		m_FunctionToMaximize = functionToMaximize;
		m_CalculatorBlueprint = calculatorBlueprint;
	}

	private int EvaluateScore(MechanicEntity entity, TargetWrapper target = null, AbilityData ability = null)
	{
		PropertyCalculatorBlueprint propertyCalculatorBlueprint = m_CalculatorBlueprint?.Value;
		if (propertyCalculatorBlueprint != null)
		{
			return propertyCalculatorBlueprint.Value.GetValue(entity, null, target, null, ability) + propertyCalculatorBlueprint.Add;
		}
		return m_FunctionToMaximize.GetValue(entity, null, target, null, ability);
	}

	public override NodeVisitResult ForwardVisit()
	{
		MechanicEntity value = m_Agent.Value;
		m_Variable.Value = GetMostValuableTarget(value, m_Entities.Value, m_Ability?.Value);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private MechanicEntity GetMostValuableTarget(MechanicEntity agent, List<MechanicEntity> targets, AbilityData ability)
	{
		if (targets == null || targets.Count == 0)
		{
			return null;
		}
		if (!HasScoringFunction)
		{
			return targets.FirstOrDefault();
		}
		return targets.MaxBy((MechanicEntity target) => EvaluateScore(agent, target, ability));
	}
}
