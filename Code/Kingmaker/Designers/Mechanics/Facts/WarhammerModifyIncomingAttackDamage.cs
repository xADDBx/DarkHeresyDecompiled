using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
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
[TypeId("c0a2f956ae0c481c8f8d1468d1ba6212")]
public class WarhammerModifyIncomingAttackDamage : BlueprintComponent
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue AdditionalAbsorption;

	public ContextValue AdditionalDeflection;

	public ContextValue AdditionalRighteousFuryChances;

	public ContextValue PercentDamageModifier;

	public bool OnlyFirstAttack;

	public bool OnlyAgainstCaster;

	public bool OnlyAgainstCasterPriorityTarget;

	public bool OnlyAgainstDirection;

	public bool ActionsOnlyOnMelee;

	public bool DoNotUseOnDOTs;

	public ActionList ActionsOnAttack;

	[SerializeField]
	[ShowIf("OnlyAgainstCasterPriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	public float Multiplier = 1f;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool MultiplyByBuffRank;

	[SerializeField]
	[ShowIf("MultiplyByBuffRank")]
	private BlueprintBuffReference m_StackingBuff;

	public bool OnlyAgainstFact;

	[SerializeField]
	[ShowIf("OnlyAgainstFact")]
	private BlueprintUnitFactReference m_CheckFact;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public BlueprintBuff StackingBuff => m_StackingBuff?.Get();

	public BlueprintUnitFact CheckFact => m_CheckFact?.Get();
}
