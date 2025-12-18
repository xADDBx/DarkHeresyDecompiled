using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("08caf9293baf6e54cafcfca35b4a5259")]
public class WarhammerOverrideAbilityStarshipWeapon : BlueprintComponent
{
	public enum OverrideMode
	{
		WithItem,
		FromWeaponWithType,
		FromPlasmaDrives
	}

	[SerializeField]
	private OverrideMode overrideMode;

	[SerializeField]
	[ShowIf("NeedShowWeaponType")]
	private StarshipWeaponType m_WeaponType;

	[SerializeField]
	[ShowIf("NeedShowItem")]
	private BlueprintStarshipWeapon.Reference m_StarshipWeapon;

	[SerializeField]
	private bool m_UseAlternativeAmmo;

	public bool NeedShowItem => overrideMode == OverrideMode.WithItem;

	public bool NeedShowWeaponType => overrideMode == OverrideMode.FromWeaponWithType;
}
