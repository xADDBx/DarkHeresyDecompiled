using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("Concentration/ChannelingAttackSettings")]
[TypeId("5400a95499104346a0351a3e62266ce7")]
public class ChannelingAttackSettings : BlueprintComponent, IAbilityAttackTypeProvider
{
	[SerializeField]
	private AbilityAttackType m_AbilityAttack;

	[SerializeField]
	private bool m_IsPrecise;

	public bool IsMelee
	{
		get
		{
			AbilityAttackType abilityAttack = m_AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleMelee || abilityAttack == AbilityAttackType.AoeMelee;
		}
	}

	public bool IsRanged
	{
		get
		{
			AbilityAttackType abilityAttack = m_AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleRanged || abilityAttack == AbilityAttackType.BurstRanged || abilityAttack == AbilityAttackType.AoeRanged;
		}
	}

	public bool IsThrow => m_AbilityAttack == AbilityAttackType.AoeThrow;

	public bool IsSingle
	{
		get
		{
			AbilityAttackType abilityAttack = m_AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleMelee || abilityAttack == AbilityAttackType.SingleRanged;
		}
	}

	public bool IsBurst => m_AbilityAttack == AbilityAttackType.BurstRanged;

	public bool IsAoe
	{
		get
		{
			AbilityAttackType abilityAttack = m_AbilityAttack;
			return abilityAttack == AbilityAttackType.AoeMelee || abilityAttack == AbilityAttackType.AoeRanged || abilityAttack == AbilityAttackType.AoeThrow;
		}
	}

	public bool IsPrecise => m_IsPrecise;
}
