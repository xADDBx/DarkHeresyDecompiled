using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
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

	public SetEntityWithMaximumValueNode(EntityVariable agent, EntityVariable variable, MechanicEntityListVariable entities, [CanBeNull] AbilityVariable ability, PropertyCalculator functionToMaximize)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Entities = entities;
		m_Ability = ability;
		m_FunctionToMaximize = functionToMaximize;
	}

	public override NodeVisitResult ForwardVisit()
	{
		MechanicEntity value = m_Agent.Value;
		m_Variable.Value = GetMostValuableTarget(value, m_Entities.Value, m_Ability?.Value, m_FunctionToMaximize);
		if (m_Variable.Value == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private MechanicEntity GetMostValuableTarget(MechanicEntity agent, List<MechanicEntity> targets, AbilityData ability, PropertyCalculator functionToMaximize)
	{
		if (targets == null || targets.Count == 0)
		{
			return null;
		}
		if (functionToMaximize == null || functionToMaximize.Empty)
		{
			return targets.FirstOrDefault();
		}
		return targets.MaxBy((MechanicEntity target) => functionToMaximize.GetValue(new PropertyContext(agent, null, target, null, ability)));
	}
}
