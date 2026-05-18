using System;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationActionHandle : AnimationActionHandle, ICloneable
{
	private AbstractUnitEntityView m_Unit;

	public new UnitAnimationAction Action => (UnitAnimationAction)base.Action;

	public new UnitAnimationManager Manager => (UnitAnimationManager)base.Manager;

	public AbstractUnitEntityView Unit
	{
		get
		{
			if (!m_Unit)
			{
				return m_Unit = Manager.GetComponentInParent<AbstractUnitEntityView>();
			}
			return m_Unit;
		}
	}

	[CanBeNull]
	public object ActionData { get; set; }

	public WeaponAnimationStyle WeaponStyle => Manager.ActiveWeaponStyle;

	public WeaponAnimationStyle EquipActionWeaponStyle { get; set; }

	public UnitEquipmentAnimationSlotType EquipmentSlot { get; set; }

	public UnitAnimationInteractionType InteractionType { get; set; }

	public BlueprintCustomLoopActionType CustomLoopActionType { get; set; }

	public bool IsBurst { get; set; }

	public AnimationAlternativeStyle AlternativeStyle { get; set; }

	public WeaponType AttackWeaponType { get; set; }

	public AttackingLimb AttackingLimb { get; set; }

	public AttackAnimationType AttackType { get; set; }

	public bool IsCornerAttack { get; set; }

	public int BurstCount { get; set; }

	public int CustomRpm { get; set; }

	public float AttackTargetDistance { get; set; }

	public bool DeathFromProne { get; set; }

	public bool DoesNotPreventMovement { get; set; }

	public bool NeedAttackAfterJump { get; set; }

	public Vector3 HitDirection { get; set; }

	public float CastingTime { get; set; }

	public float JumpTime { get; set; }

	public AbilityAnimationStyle AbilityStyle { get; set; }

	public bool CastInOffhand { get; set; }

	public bool IsBladeDance { get; set; }

	public bool IsTargetSelf { get; set; }

	public bool IsPrecastFinished { get; set; }

	public BlueprintAbility Spell { get; set; }

	public SpecialBehaviourType SpecialCastBehaviour { get; set; }

	public AnimationClipWrapper AnimationClipWrapper { get; set; }

	public int Variant { get; set; }

	public int SpecialAttackCount { get; set; }

	public BlueprintRace UnitRace => Manager.AnimationRace;

	public UnitAnimationActionHandle(UnitAnimationAction action, UnitAnimationManager manager)
		: base(action, manager)
	{
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
