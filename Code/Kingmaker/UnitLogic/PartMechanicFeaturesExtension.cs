using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.UnitLogic;

public static class PartMechanicFeaturesExtension
{
	public static bool HasMechanicFeature(this MechanicEntity entity, MechanicsFeatureType type)
	{
		return entity.GetMechanicFeature(type);
	}

	public static FeatureCountableFlag GetMechanicFeature(this MechanicEntity entity, MechanicsFeatureType type)
	{
		PartMechanicFeatures features = entity.Features;
		return type switch
		{
			MechanicsFeatureType.Invalid => throw new Exception("Invalid type"), 
			MechanicsFeatureType.CantAct => features.CantAct, 
			MechanicsFeatureType.CantMove => features.CantMove, 
			MechanicsFeatureType.Undying => features.Undying, 
			MechanicsFeatureType.Flying => features.Flying, 
			MechanicsFeatureType.IsUntargetable => features.IsUntargetable, 
			MechanicsFeatureType.Immortality => features.Immortality, 
			MechanicsFeatureType.AllowDyingCondition => features.AllowDyingCondition, 
			MechanicsFeatureType.IsIgnoredByCombat => features.IsIgnoredByCombat, 
			MechanicsFeatureType.Hidden => features.Hidden, 
			MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity => features.DoNotProvokeAttacksOfOpportunity, 
			MechanicsFeatureType.DoNotResetMovementPointsOnAttacks => features.DoNotResetMovementPointsOnAttacks, 
			MechanicsFeatureType.DisableAttacksOfOpportunity => features.DisableAttacksOfOpportunity, 
			MechanicsFeatureType.IgnoreThreateningAreaForMovementCostCalculation => features.IgnoreThreateningAreaForMovementCostCalculation, 
			MechanicsFeatureType.DisablePush => features.DisablePush, 
			MechanicsFeatureType.DoNotHealOutOfCombat => features.DoNotHealOutOfCombat, 
			MechanicsFeatureType.AutoHit => features.AutoHit, 
			MechanicsFeatureType.AutoMiss => features.AutoMiss, 
			MechanicsFeatureType.CanRerollSavingThrow => features.CanRerollSavingThrow, 
			MechanicsFeatureType.CanShootInMelee => features.CanShootInMelee, 
			MechanicsFeatureType.Vanguard => features.Vanguard, 
			MechanicsFeatureType.CanPassThroughUnits => features.CanPassThroughUnits, 
			MechanicsFeatureType.CannotBeCriticallyHit => features.CannotBeCriticallyHit, 
			MechanicsFeatureType.HealCanCrit => features.HealCanCrit, 
			MechanicsFeatureType.IgnoreWeaponForceMove => features.IgnoreWeaponForceMove, 
			MechanicsFeatureType.IgnoreAnyForceMove => features.IgnoreAnyForceMove, 
			MechanicsFeatureType.CantJumpAside => features.CantJumpAside, 
			MechanicsFeatureType.FreshInjuryImmunity => features.FreshInjuryImmunity, 
			MechanicsFeatureType.ImmuneToMovementPointReduction => features.ImmuneToMovementPointReduction, 
			MechanicsFeatureType.SecondaryCriticalChance => features.SecondaryCriticalChance, 
			MechanicsFeatureType.OverpenetrationDoesNotDecreaseDamage => features.OverpenetrationDoesNotDecreaseDamage, 
			MechanicsFeatureType.IgnoreMeleeOutnumbering => features.IgnoreMeleeOutnumbering, 
			MechanicsFeatureType.DoesNotCountTurns => features.DoesNotCountTurns, 
			MechanicsFeatureType.HasNoAPPenaltyCostForTwoWeaponFighting => features.HasNoAPPenaltyCostForTwoWeaponFighting, 
			MechanicsFeatureType.IgnoreCoverEfficiency => features.IgnoreCoverEfficiency, 
			MechanicsFeatureType.WitheringShard => features.WitheringShard, 
			MechanicsFeatureType.HiveOutnumber => features.HiveOutnumber, 
			MechanicsFeatureType.PsychicPowersDoNotProvokeAoO => features.PsychicPowersDoNotProvokeAoO, 
			MechanicsFeatureType.RemoveFromInitiative => features.RemoveFromInitiative, 
			MechanicsFeatureType.BlockOverpenetration => features.BlockOverpenetration, 
			MechanicsFeatureType.OnElevator => features.OnElevator, 
			MechanicsFeatureType.RotationForbidden => features.RotationForbidden, 
			MechanicsFeatureType.SuppressedDismember => features.SuppressedDismember, 
			MechanicsFeatureType.SuppressedDecomposition => features.SuppressedDecomposition, 
			MechanicsFeatureType.HealInsteadOfDamageForDOTs => features.HealInsteadOfDamageForDOTs, 
			MechanicsFeatureType.CanUseBothDesperateMeasureAndHeroicAct => features.CanUseBothDesperateMeasureAndHeroicAct, 
			MechanicsFeatureType.HalfSuperiorityCriticalChance => features.HalfSuperiorityCriticalChance, 
			MechanicsFeatureType.HideRealHealthInUI => features.HideRealHealthInUI, 
			MechanicsFeatureType.MoraleLeader => features.MoraleLeader, 
			MechanicsFeatureType.Prone => features.Prone, 
			MechanicsFeatureType.Stunned => features.Stunned, 
			MechanicsFeatureType.Sleeping => features.Sleeping, 
			MechanicsFeatureType.IsCharging => features.IsCharging, 
			MechanicsFeatureType.ControlledByDirector => features.ControlledByDirector, 
			MechanicsFeatureType.PsykerFetter => features.PsykerFetter, 
			MechanicsFeatureType.PsykerPush => features.PsykerPush, 
			MechanicsFeatureType.MoraleDisposable => features.MoraleDisposable, 
			MechanicsFeatureType.MoraleCanNotGainMorale => features.MoraleCanNotGainMorale, 
			MechanicsFeatureType.DefenceDisabled => features.DefenceDisabled, 
			MechanicsFeatureType.CanNotMoveCloserToEnemies => features.CanNotMoveCloserToEnemies, 
			MechanicsFeatureType.CanDeployNearEnemies => features.CanDeployNearEnemies, 
			MechanicsFeatureType.FreeMovementNearParty => features.FreeMovementNearParty, 
			MechanicsFeatureType.BurstFirePointTarget => features.BurstFirePointTarget, 
			MechanicsFeatureType.AttacksIgnoreAllies => features.AttacksIgnoreAllies, 
			MechanicsFeatureType.AllowDefenceAgainstRangedAttacksWithAoe => features.AllowDefenceAgainstRangedAttacksWithAoe, 
			MechanicsFeatureType.IgnoreAlliedAoeAttacks => features.IgnoreAlliedAoeAttacks, 
			MechanicsFeatureType.HitRandomBodyPartWithAoeRanged => features.HitRandomBodyPartWithAoeRanged, 
			MechanicsFeatureType.HitRandomBodyPartWithAoeThrow => features.HitRandomBodyPartWithAoeThrow, 
			MechanicsFeatureType.AllowAttackOfOpportunityWithRangedWeapon => features.AllowAttackOfOpportunityWithRangedWeapon, 
			MechanicsFeatureType.SteadyConcentration => features.SteadyConcentration, 
			MechanicsFeatureType.DualWielderAttackOfOpportunity => features.DualWielderAttackOfOpportunity, 
			MechanicsFeatureType.PreciseAttackAutoHit => features.PreciseAttackAutoHit, 
			MechanicsFeatureType.DoesNotSurrender => features.DoesNotSurrender, 
			MechanicsFeatureType.AutoDefenceAgainstAllyMeleeAttacks => features.AutoDefenceAgainstAllyMeleeAttacks, 
			MechanicsFeatureType.DoNotUseMorale => features.DoNotUseMoraleAndPowerBalance, 
			MechanicsFeatureType.CanMoveThroughEnemies => features.CanMoveThroughEnemies, 
			MechanicsFeatureType.PassThroughSmallUnits => features.PassThroughSmallUnits, 
			MechanicsFeatureType.CanAoODuringOwnTurn => features.CanAoODuringOwnTurn, 
			MechanicsFeatureType.IgnoreDefence => features.IgnoreDefence, 
			MechanicsFeatureType.CantMoveInCombat => features.CantMoveInCombat, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
