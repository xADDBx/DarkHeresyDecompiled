using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("06b0345091976784cb849e08a5c6c1f6")]
public class StarshipDamageAdjacentShields : BlueprintComponent
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen;

	public bool CheckWeaponBlueprint;

	[SerializeField]
	[ShowIf("CheckWeaponBlueprint")]
	private BlueprintStarshipWeapon.Reference[] m_WeaponBlueprints;

	public ReferenceArrayProxy<BlueprintStarshipWeapon> WeaponBlueprints
	{
		get
		{
			BlueprintReference<BlueprintStarshipWeapon>[] weaponBlueprints = m_WeaponBlueprints;
			return weaponBlueprints;
		}
	}
}
