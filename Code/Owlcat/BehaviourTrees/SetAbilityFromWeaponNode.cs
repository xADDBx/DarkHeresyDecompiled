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

	public SetAbilityFromWeaponNode(EntityVariable agent, AbilityVariable variable, WeaponHandType hand)
	{
		m_Agent = agent;
		m_Variable = variable;
		m_Hand = hand;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = GetAbility((BaseUnitEntity)m_Agent.Value, m_Hand);
		if (!(m_Variable.Value != null))
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}

	private static AbilityData GetAbility(BaseUnitEntity caster, WeaponHandType hand)
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
		return itemEntityWeapon.Abilities[0]?.Data;
	}
}
