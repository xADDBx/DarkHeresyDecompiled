using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("eae609680122440d89d28229f76d17ff")]
public class WarhammerDamageOverTimeCriticalHit : BlueprintComponent
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	[SerializeField]
	private BlueprintAbilityGroupReference m_DamageOverTimeGroup;

	[SerializeField]
	private BlueprintAbilityReference m_BaseAbility;

	public BlueprintAbilityGroup DamageOverTimeGroup => m_DamageOverTimeGroup;

	public BlueprintAbility BaseAbility => m_BaseAbility;
}
