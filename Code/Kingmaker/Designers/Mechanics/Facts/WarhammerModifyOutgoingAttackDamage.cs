using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("28a78c842799f6b42bb3970a9fd77371")]
public class WarhammerModifyOutgoingAttackDamage : BlueprintComponent
{
	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue AdditionalAbsorption;

	public ContextValue AdditionalDeflection;

	public ContextValue AdditionalRighteousFuryChances;

	public bool OnlyFirstAttack;

	public bool OnlyFirstAttackAgainstEveryTarget;

	public bool OnlyAgainstCaster;

	public bool OnlyAgainstPriorityTarget;

	public bool ActionsOnlyOnMelee;

	public bool ActionsOnlyOnFirstAttack;

	public bool DoNotUseOnDOTs;

	public ActionList ActionsOnAttack;

	[SerializeField]
	[ShowIf("OnlyAgainstPriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	public float Multiplier = 1f;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool SpecificWeaponFamily;

	[ShowIf("SpecificWeaponFamily")]
	public WeaponFamily WeaponFamily = WeaponFamily.Bolt;

	public bool OnlyChosenWeapon;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();
}
