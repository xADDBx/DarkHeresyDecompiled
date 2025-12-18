using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ce1b1337842ceb1408b9e01185002025")]
public class WarhammerReflectDamage : WarhammerDamageTrigger
{
	[SerializeField]
	[HideIf("UseValueInstead")]
	private ContextValue m_Percentage;

	[SerializeField]
	[ShowIf("UseValueInstead")]
	private ContextValue m_Value;

	public bool UseValueInstead;

	public bool ChangeReflectedDamageType;

	[SerializeField]
	[ShowIf("ChangeReflectedDamageType")]
	private DamageType m_Type;

	public void OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
	}
}
