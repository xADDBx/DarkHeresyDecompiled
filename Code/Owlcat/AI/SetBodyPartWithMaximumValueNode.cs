using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
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

	public SetBodyPartWithMaximumValueNode(EntityVariable agent, BodyPartVariable variable, BodyPartListVariable bodyPartsList, EntityVariable target, PropertyCalculator functionToMaximize, int minThresholdValue)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_BodyPartsList = bodyPartsList;
		m_Target = target;
		m_FunctionToMaximize = functionToMaximize;
		m_MinThresholdValue = minThresholdValue;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetGraphNodeWithMaximumValue(m_Agent.Value, (BaseUnitEntity)m_Target.Value, m_BodyPartsList.Value, m_FunctionToMaximize, m_MinThresholdValue);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static BpRef<BlueprintBodyPart> GetGraphNodeWithMaximumValue(MechanicEntity agent, BaseUnitEntity target, List<BpRef<BlueprintBodyPart>> bodyParts, PropertyCalculator utilityFunction, int minThresholdValue)
	{
		if (bodyParts == null || bodyParts.Count == 0)
		{
			return null;
		}
		if (utilityFunction == null || utilityFunction.Empty)
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
					num2 += utilityFunction.GetValue(new PropertyContext(agent, null, target));
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
