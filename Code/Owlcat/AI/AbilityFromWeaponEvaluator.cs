using System;
using Kingmaker;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.BehaviourTrees;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/AbilityFromWeaponEvaluator")]
[TypeId("bc48016cd34f5c24a90c5c365bfffbb3")]
public class AbilityFromWeaponEvaluator : AbilityEvaluator
{
	[SerializeReference]
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	[SerializeField]
	public WeaponHandType WeaponHand;

	[SerializeField]
	public AbilityType AbilityType;

	public override string GetCaption()
	{
		return $"Ability from {WeaponHand} ({AbilityType}) of [{Entity}]";
	}

	protected override AbilityData GetValueInternal()
	{
		if (!Entity.TryGetValue(out var value))
		{
			return null;
		}
		if (!(value is BaseUnitEntity baseUnitEntity))
		{
			PFLog.AI.Error($"{value} is not BaseUnitEntity");
			return null;
		}
		ItemEntityWeapon itemEntityWeapon;
		switch (WeaponHand)
		{
		case WeaponHandType.Primary:
			itemEntityWeapon = baseUnitEntity.GetFirstWeapon();
			break;
		case WeaponHandType.Secondary:
			itemEntityWeapon = baseUnitEntity.GetSecondWeapon();
			break;
		default:
			PFLog.AI.Error($"Unknown WeaponHandType: {WeaponHand}");
			return null;
		}
		if (itemEntityWeapon == null)
		{
			PFLog.AI.Error($"No weapon in {WeaponHand}");
			return null;
		}
		if (itemEntityWeapon.Abilities.Count == 0)
		{
			return null;
		}
		if (AbilityType == AbilityType.Any)
		{
			return itemEntityWeapon.Abilities[0]?.Data;
		}
		AbilityType abilityType = AbilityType;
		if ((uint)(abilityType - 1) > 2u)
		{
			PFLog.AI.Error($"Unknown AbilityType: {AbilityType}");
			return null;
		}
		for (int i = 0; i < itemEntityWeapon.Abilities.Count; i++)
		{
			Ability ability = itemEntityWeapon.Abilities[i];
			if (AbilityType switch
			{
				AbilityType.Burst => ability.Blueprint.IsBurst, 
				AbilityType.AOE => ability.Blueprint.IsAoE, 
				AbilityType.SingleShot => !ability.Blueprint.IsAoE && !ability.Blueprint.IsBurst, 
				_ => false, 
			})
			{
				return ability.Data;
			}
		}
		return null;
	}
}
