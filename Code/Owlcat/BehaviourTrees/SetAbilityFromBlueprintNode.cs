using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Owlcat.BehaviourTrees;

public class SetAbilityFromBlueprintNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly AbilityVariable m_Variable;

	private readonly BlueprintAbility m_Blueprint;

	public SetAbilityFromBlueprintNode(EntityVariable agent, AbilityVariable variable, BlueprintAbility blueprintAbility)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Blueprint = blueprintAbility;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetAbility((BaseUnitEntity)m_Agent.Value, m_Blueprint);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static AbilityData GetAbility(BaseUnitEntity caster, BlueprintAbility blueprint)
	{
		if (blueprint == null)
		{
			PFLog.AI.Error("BlueprintAbility is null");
			return null;
		}
		Ability ability = caster.Abilities.GetAbility(blueprint);
		if (ability == null)
		{
			PFLog.AI.Log($"{caster} doesn't have {blueprint.name} ability");
			return null;
		}
		return ability.Data;
	}
}
