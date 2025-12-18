using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("c2129f96be33c7e45917aabea8b92623")]
public class AddOutgoingDamageTrigger : BlueprintComponent
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool WasTargetAlive;
	}

	public ActionList Actions;

	public bool TriggerOnStatDamageOrEnergyDrain;

	public bool CheckAbilityType;

	[ShowIf("CheckAbilityType")]
	public AbilityType m_AbilityType;

	public bool CheckSpellDescriptor;

	public bool CheckSpellParent;

	public bool NotZeroDamage;

	public bool CheckDamageType;

	[ShowIf("CheckDamageType")]
	public DamageType DamageType;

	public bool ApplyToAreaEffectDamage;

	public bool TargetKilledByThisDamage;

	public bool TargetHasFact;

	[ShowIf("CheckSpellParent")]
	[SerializeField]
	private BlueprintAbilityReference[] m_AbilityList;

	[ShowIf("CheckSpellDescriptor")]
	public SpellDescriptorWrapper SpellDescriptorsList;

	public bool OnlyMelee;

	public bool ActionsOnInitiator;

	public bool TriggersForDamageOverTime;

	[SerializeField]
	private BlueprintUnitFactReference[] m_TargetFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> TargetFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] targetFacts = m_TargetFacts;
			return targetFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintAbility> AbilityList
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] abilityList = m_AbilityList;
			return abilityList;
		}
	}
}
