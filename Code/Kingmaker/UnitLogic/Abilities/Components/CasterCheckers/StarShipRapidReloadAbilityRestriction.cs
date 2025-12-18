using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("87ccb62e8f0bf2e498c93ba5d4f24b73")]
public class StarShipRapidReloadAbilityRestriction : BlueprintComponent
{
	public bool AllowPenalted;

	public bool AllowReloadAtAccelerationPhase;

	[ShowIf("AllowReloadAtAccelerationPhase")]
	public StarshipWeaponType ReloadWeapon;

	[SerializeField]
	[ShowIf("AllowReloadAtAccelerationPhase")]
	private BlueprintBuffReference m_ReloadBlockingBuff;

	public BlueprintBuff ReloadBlockingBuff => m_ReloadBlockingBuff?.Get();
}
