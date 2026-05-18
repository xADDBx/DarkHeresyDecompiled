using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.Predictions;

public struct AbilityTargetUIData : IEquatable<AbilityTargetUIData>
{
	private Vector3 CasterPosition { get; }

	private BlueprintBodyPart BodyPart { get; }

	public AbilityData Ability { get; }

	public MechanicEntity Target { get; }

	public UIHitChancePredictionData HitChance { get; private set; }

	public UIDamagePredictionData Damage { get; private set; }

	public UIHealPredictionData Heal { get; }

	public UIMoralePredictionData Morale { get; }

	public UIBuffsPredictionData Buffs { get; }

	public int AttacksCount { get; }

	public bool CanTargetFromDesiredPosition { get; }

	public AbilityTargetUIData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, BlueprintBodyPart bodyPart, UIHitChancePredictionData hitChancePrediction, UIDamagePredictionData damagePrediction, UIHealPredictionData healPrediction, UIMoralePredictionData moralePrediction, UIBuffsPredictionData buffsPrediction)
	{
		Ability = ability;
		CasterPosition = casterPosition;
		Target = target;
		BodyPart = bodyPart;
		HitChance = hitChancePrediction;
		Damage = damagePrediction;
		Heal = healPrediction;
		Morale = moralePrediction;
		Buffs = buffsPrediction;
		AttacksCount = ability.BurstAttacksCount;
		CanTargetFromDesiredPosition = hitChancePrediction.CanTargetFromDesiredPosition;
	}

	public void UpdateSingleAttack(UIDamagePredictionData damage, UIHitChancePredictionData hitChance)
	{
		Damage = damage;
		HitChance = hitChance;
	}

	public bool Equals(AbilityTargetUIData other)
	{
		if (object.Equals(Ability, other.Ability) && object.Equals(BodyPart, other.BodyPart) && object.Equals(Target, other.Target) && CasterPosition.Equals(other.CasterPosition) && HitChance.Equals(other.HitChance) && Damage.Equals(other.Damage) && Morale.Equals(other.Morale))
		{
			return Heal.Equals(other.Heal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AbilityTargetUIData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Ability, Target, CasterPosition, BodyPart);
	}

	public static bool operator ==(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return !left.Equals(right);
	}
}
