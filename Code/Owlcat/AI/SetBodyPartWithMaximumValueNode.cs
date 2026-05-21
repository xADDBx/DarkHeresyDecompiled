using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility;
using Owlcat.AI.Mechanics.BodyParts;
using Owlcat.BehaviourTrees;
using Owlcat.Fmw.Blueprints;

namespace Owlcat.AI;

public class SetBodyPartWithMaximumValueNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly BodyPartVariable m_Variable;

	private readonly BodyPartListVariable m_BodyPartsList;

	private readonly EntityVariable m_Target;

	private readonly PropertyCalculator m_FunctionToMaximize;

	private readonly int m_MinThresholdValue;

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

	public SetBodyPartWithMaximumValueNode(EntityVariable agent, BodyPartVariable variable, BodyPartListVariable bodyPartsList, EntityVariable target, PropertyCalculator functionToMaximize, int minThresholdValue, PropertyCalculatorBlueprintVariable calculatorBlueprint)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_BodyPartsList = bodyPartsList;
		m_Target = target;
		m_FunctionToMaximize = functionToMaximize;
		m_MinThresholdValue = minThresholdValue;
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
		m_Variable.Value = GetBodyPartWithMaximumValue(m_Agent.Value, (BaseUnitEntity)m_Target.Value, m_BodyPartsList.Value, m_MinThresholdValue);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private BpRef<BlueprintBodyPart> GetBodyPartWithMaximumValue(MechanicEntity agent, BaseUnitEntity target, List<BpRef<BlueprintBodyPart>> bodyParts, int minThresholdValue)
	{
		if (bodyParts == null || bodyParts.Count == 0)
		{
			return null;
		}
		if (!HasScoringFunction)
		{
			return null;
		}
		BlueprintBodyPart blueprint = null;
		int num = int.MinValue;
		foreach (BpRef<BlueprintBodyPart> bodyPart in bodyParts)
		{
			if (target.BodyParts.Contains(bodyPart.Blueprint))
			{
				int num2 = 0;
				using (ContextData<AiBodyPartsContextData>.Request().Setup(bodyPart.Blueprint))
				{
					num2 += EvaluateScore(agent, target);
				}
				if (num2 > num)
				{
					num = num2;
					blueprint = bodyPart;
				}
			}
		}
		if (num < minThresholdValue)
		{
			return null;
		}
		return blueprint.Reference();
	}
}
