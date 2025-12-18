using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class PartMechanicFeatures : MechanicEntityPart, IHashable, IOwlPackable<PartMechanicFeatures>
{
	public FeatureCountableFlag Undying;

	public FeatureCountableFlag DoNotProvokeAttacksOfOpportunity;

	public FeatureCountableFlag Flying;

	public FeatureCountableFlag IsUntargetable;

	public FeatureCountableFlag Immortality;

	public FeatureCountableFlag AllowDyingCondition;

	public FeatureCountableFlag IsIgnoredByCombat;

	public FeatureCountableFlag Hidden;

	public FeatureCountableFlag OnElevator;

	public FeatureCountableFlag DoNotResetMovementPointsOnAttacks;

	public FeatureCountableFlag RotationForbidden;

	public FeatureCountableFlag IgnoreThreateningAreaForMovementCostCalculation;

	public FeatureCountableFlag DisablePush;

	public FeatureCountableFlag DoNotReviveOutOfCombat;

	public FeatureCountableFlag AutoHit;

	public FeatureCountableFlag AutoMiss;

	public FeatureCountableFlag CanRerollSavingThrow;

	public FeatureCountableFlag CanShootInMelee;

	public FeatureCountableFlag CanPassThroughUnits;

	public FeatureCountableFlag CannotBeCriticallyHit;

	public FeatureCountableFlag HealCanCrit;

	public FeatureCountableFlag IgnoreWeaponForceMove;

	public FeatureCountableFlag IgnoreAnyForceMove;

	public FeatureCountableFlag CantJumpAside;

	public FeatureCountableFlag FreshInjuryImmunity;

	public FeatureCountableFlag ImmuneToMovementPointReduction;

	public FeatureCountableFlag SecondaryCriticalChance;

	public FeatureCountableFlag OverpenetrationDoesNotDecreaseDamage;

	public FeatureCountableFlag IsFirstInFight;

	public FeatureCountableFlag IsLastInFight;

	public FeatureCountableFlag IgnoreMeleeOutnumbering;

	public FeatureCountableFlag DoesNotCountTurns;

	public FeatureCountableFlag HasNoAPPenaltyCostForTwoWeaponFighting;

	public FeatureCountableFlag IgnoreCoverEfficiency;

	public FeatureCountableFlag WitheringShard;

	public FeatureCountableFlag HiveOutnumber;

	public FeatureCountableFlag PsychicPowersDoNotProvokeAoO;

	public FeatureCountableFlag BlockOverpenetration;

	public FeatureCountableFlag HealInsteadOfDamageForDOTs;

	public FeatureCountableFlag CanUseBothDesperateMeasureAndHeroicAct;

	public FeatureCountableFlag HalfSuperiorityCriticalChance;

	public FeatureCountableFlag HideRealHealthInUI;

	public FeatureCountableFlag MoraleLeader;

	public FeatureCountableFlag ControlledByDirector;

	public FeatureCountableFlag PsykerFetter;

	public FeatureCountableFlag PsykerPush;

	public FeatureCountableFlag MoraleDisposable;

	public FeatureCountableFlag MoraleCanNotGainMorale;

	public FeatureCountableFlag DefenceDisabled;

	public FeatureCountableFlag CanNotMoveCloserToEnemies;

	public FeatureCountableFlag CanDeployNearEnemies;

	public FeatureCountableFlag FreeMovementNearParty;

	public FeatureCountableFlag BurstFirePointTarget;

	public FeatureCountableFlag AttacksIgnoreAllies;

	public FeatureCountableFlag AllowDefenceAgainstRangedAttacksWithAoe;

	public FeatureCountableFlag IgnoreAlliedAoeAttacks;

	public FeatureCountableFlag HitRandomBodyPartWithAoeRanged;

	public FeatureCountableFlag HitRandomBodyPartWithAoeThrow;

	public FeatureCountableFlag AllowAttackOfOpportunityWithRangedWeapon;

	public FeatureCountableFlag SteadyConcentration;

	public FeatureCountableFlag DualWielderAttackOfOpportunity;

	public FeatureCountableFlag PreciseAttackAutoHit;

	public FeatureCountableFlag DoesNotSurrender;

	public FeatureCountableFlag AutoDefenceAgainstAllyMeleeAttacks;

	public FeatureCountableFlag DoNotUseMoraleAndPowerBalance;

	public FeatureCountableFlag CanMoveThroughEnemies;

	public FeatureCountableFlag PassThroughSmallUnits;

	public FeatureCountableFlag CanAoODuringOwnTurn;

	public FeatureCountableFlag IgnoreDefence;

	public FeatureCountableFlag Prone;

	public FeatureCountableFlag Stunned;

	public FeatureCountableFlag Sleeping;

	public FeatureCountableFlag CantAct;

	public FeatureCountableFlag CantMove;

	public FeatureCountableFlag DisableAttacksOfOpportunity;

	public FeatureCountableFlag Vanguard;

	public FeatureCountableFlag RemoveFromInitiative;

	public FeatureCountableFlag IsCharging;

	public FeatureCountableFlag SuppressedDismember;

	public FeatureCountableFlag SuppressedDecomposition;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMechanicFeatures",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Initialize()
	{
		Undying = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Undying);
		DoNotProvokeAttacksOfOpportunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity);
		Flying = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Flying);
		IsUntargetable = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsUntargetable);
		Immortality = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Immortality);
		AllowDyingCondition = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AllowDyingCondition);
		IsIgnoredByCombat = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsIgnoredByCombat);
		Hidden = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Hidden);
		OnElevator = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.OnElevator);
		DoNotResetMovementPointsOnAttacks = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotResetMovementPointsOnAttacks);
		RotationForbidden = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.RotationForbidden);
		IgnoreThreateningAreaForMovementCostCalculation = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreThreateningAreaForMovementCostCalculation);
		DisablePush = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DisablePush);
		CantAct = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantAct);
		CantMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantMove);
		DisableAttacksOfOpportunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DisableAttacksOfOpportunity);
		SuppressedDismember = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SuppressedDismember);
		SuppressedDecomposition = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SuppressedDecomposition);
		DoNotReviveOutOfCombat = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotReviveOutOfCombat);
		AutoHit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoHit);
		AutoMiss = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoMiss);
		CanRerollSavingThrow = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanRerollSavingThrow);
		CanShootInMelee = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanShootInMelee);
		Vanguard = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Vanguard);
		CanPassThroughUnits = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanPassThroughUnits);
		CannotBeCriticallyHit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CannotBeCriticallyHit);
		HealCanCrit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HealCanCrit);
		IgnoreWeaponForceMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreWeaponForceMove);
		IgnoreAnyForceMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreAnyForceMove);
		CantJumpAside = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantJumpAside);
		FreshInjuryImmunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.FreshInjuryImmunity);
		ImmuneToMovementPointReduction = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.ImmuneToMovementPointReduction);
		SecondaryCriticalChance = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SecondaryCriticalChance);
		OverpenetrationDoesNotDecreaseDamage = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.OverpenetrationDoesNotDecreaseDamage);
		IsFirstInFight = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsFirstInFight);
		IsLastInFight = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsLastInFight);
		IgnoreMeleeOutnumbering = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreMeleeOutnumbering);
		DoesNotCountTurns = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoesNotCountTurns);
		HasNoAPPenaltyCostForTwoWeaponFighting = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HasNoAPPenaltyCostForTwoWeaponFighting);
		IgnoreCoverEfficiency = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreCoverEfficiency);
		WitheringShard = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.WitheringShard);
		HiveOutnumber = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HiveOutnumber);
		PsychicPowersDoNotProvokeAoO = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PsychicPowersDoNotProvokeAoO);
		RemoveFromInitiative = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.RemoveFromInitiative);
		BlockOverpenetration = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.BlockOverpenetration);
		HealInsteadOfDamageForDOTs = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HealInsteadOfDamageForDOTs);
		CanUseBothDesperateMeasureAndHeroicAct = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanUseBothDesperateMeasureAndHeroicAct);
		HalfSuperiorityCriticalChance = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HalfSuperiorityCriticalChance);
		HideRealHealthInUI = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HideRealHealthInUI);
		MoraleLeader = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.MoraleLeader);
		Prone = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Prone);
		Stunned = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Stunned);
		Sleeping = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Sleeping);
		IsCharging = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsCharging);
		ControlledByDirector = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.ControlledByDirector);
		PsykerFetter = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PsykerFetter);
		PsykerPush = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PsykerPush);
		MoraleDisposable = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.MoraleDisposable);
		MoraleCanNotGainMorale = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.MoraleCanNotGainMorale);
		DefenceDisabled = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DefenceDisabled);
		CanNotMoveCloserToEnemies = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanNotMoveCloserToEnemies);
		CanDeployNearEnemies = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanDeployNearEnemies);
		FreeMovementNearParty = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.FreeMovementNearParty);
		BurstFirePointTarget = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.BurstFirePointTarget);
		AttacksIgnoreAllies = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AttacksIgnoreAllies);
		AllowDefenceAgainstRangedAttacksWithAoe = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AllowDefenceAgainstRangedAttacksWithAoe);
		IgnoreAlliedAoeAttacks = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreAlliedAoeAttacks);
		HitRandomBodyPartWithAoeRanged = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HitRandomBodyPartWithAoeRanged);
		HitRandomBodyPartWithAoeThrow = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HitRandomBodyPartWithAoeThrow);
		AllowAttackOfOpportunityWithRangedWeapon = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AllowAttackOfOpportunityWithRangedWeapon);
		SteadyConcentration = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SteadyConcentration);
		DualWielderAttackOfOpportunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DualWielderAttackOfOpportunity);
		PreciseAttackAutoHit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PreciseAttackAutoHit);
		DoesNotSurrender = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoesNotSurrender);
		AutoDefenceAgainstAllyMeleeAttacks = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoDefenceAgainstAllyMeleeAttacks);
		DoNotUseMoraleAndPowerBalance = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotUseMorale);
		CanMoveThroughEnemies = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanMoveThroughEnemies);
		PassThroughSmallUnits = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PassThroughSmallUnits);
		CanAoODuringOwnTurn = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanAoODuringOwnTurn);
		IgnoreDefence = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreDefence);
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		Initialize();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartMechanicFeatures source = new PartMechanicFeatures();
		result = Unsafe.As<PartMechanicFeatures, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<PartMechanicFeatures>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMechanicFeatures>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
