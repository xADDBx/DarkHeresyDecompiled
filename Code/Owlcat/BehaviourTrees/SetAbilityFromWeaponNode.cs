using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;

namespace Owlcat.BehaviourTrees;

public class SetAbilityFromWeaponNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly AbilityVariable m_Variable;

	private readonly WeaponHandType m_Hand;

	private readonly AbilityType m_AbilityType;

	public SetAbilityFromWeaponNode(EntityVariable agent, AbilityVariable variable, WeaponHandType hand, AbilityType abilityType)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Hand = hand;
		m_AbilityType = abilityType;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetAbility((BaseUnitEntity)m_Agent.Value, m_Hand, m_AbilityType);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static AbilityData GetAbility(BaseUnitEntity caster, WeaponHandType hand, AbilityType abilityType)
	{
		ItemEntityWeapon itemEntityWeapon = hand switch
		{
			WeaponHandType.Primary => caster.GetFirstWeapon(), 
			WeaponHandType.Secondary => caster.GetSecondWeapon(), 
			_ => caster.GetFirstWeapon(), 
		};
		if (itemEntityWeapon == null)
		{
			PFLog.AI.Error($"No weapon in {hand}");
			return null;
		}
		return abilityType switch
		{
			AbilityType.Any => itemEntityWeapon.Abilities[0]?.Data, 
			AbilityType.Burst => itemEntityWeapon.Abilities.FirstOrDefault((Ability a) => a.Blueprint.IsBurst)?.Data, 
			AbilityType.AOE => itemEntityWeapon.Abilities.FirstOrDefault((Ability a) => a.Blueprint.IsAoE)?.Data, 
			AbilityType.SingleShot => itemEntityWeapon.Abilities.FirstOrDefault((Ability a) => !a.Blueprint.IsAoE && !a.Blueprint.IsBurst)?.Data, 
			_ => null, 
		};
	}
}
