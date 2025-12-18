using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f332e1a348e0aab40924f7a450d7c484")]
public class StarshipPerformAttackTrigger : BlueprintComponent
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget,
		Both
	}

	private enum AEType
	{
		Ignore,
		Require,
		Exclude
	}

	[SerializeField]
	private bool PerformActionsOnHullDamagePortion;

	[SerializeField]
	private bool PerformActionsOnShieldsDamagePortion;

	[SerializeField]
	private bool PerformActionsOnKill;

	[SerializeField]
	private bool PerformActionsOnSurvive;

	[SerializeField]
	[ShowIf("ShouldPerformActionsOnAnyDamage")]
	private int PercentOfMaxDamageNeededForActions;

	[SerializeField]
	private TriggerType triggerType;

	[SerializeField]
	private AEType aeType;

	[Tooltip("Instead of triggering on each attack in burst, trigger will happen on last attack when at last one of the burst attacks passed the checks")]
	[SerializeField]
	private bool AggregateBurst;

	[SerializeField]
	private bool CheckInitiatorFaction;

	[SerializeField]
	[ShowIf("CheckInitiatorFaction")]
	private BlueprintFactionReference m_Faction;

	public bool CheckWeaponBlueprint;

	[SerializeField]
	[ShowIf("CheckWeaponBlueprint")]
	private BlueprintStarshipWeapon.Reference[] m_WeaponBlueprints;

	[SerializeField]
	private ActionList Actions;

	[SerializeField]
	private ActionList TargetUnitActions;

	[SerializeField]
	[ShowIf("IsAttachedToAbility")]
	private bool TriggerForThisAbilityOnly = true;

	public ReferenceArrayProxy<BlueprintStarshipWeapon> WeaponBlueprints
	{
		get
		{
			BlueprintReference<BlueprintStarshipWeapon>[] weaponBlueprints = m_WeaponBlueprints;
			return weaponBlueprints;
		}
	}

	public BlueprintFaction Faction => m_Faction?.Get();

	private bool ShouldPerformActionsOnAnyDamage
	{
		get
		{
			if (!PerformActionsOnHullDamagePortion)
			{
				return PerformActionsOnShieldsDamagePortion;
			}
			return true;
		}
	}

	private bool HasAnyConditionsToPerformActions
	{
		get
		{
			if (!ShouldPerformActionsOnAnyDamage && !PerformActionsOnKill)
			{
				return PerformActionsOnSurvive;
			}
			return true;
		}
	}

	private bool IsAttachedToAbility => base.OwnerBlueprint is BlueprintAbility;
}
