using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("858abf54f50b4ca4da20cf6962751354")]
public class StarshipHitCritModifier : BlueprintComponent
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen;

	public int HitBonusPct;

	public int CritBonusPct;

	public float CritBonusMod;

	public bool CheckWeaponType;

	[ShowIf("CheckWeaponType")]
	public StarshipWeaponType WeaponType;

	public bool CheckWeaponBlueprint;

	[SerializeField]
	[ShowIf("CheckWeaponBlueprint")]
	private BlueprintStarshipWeapon.Reference[] m_WeaponBlueprints;

	[SerializeField]
	private BlueprintFeatureReference[] m_TargetAnyFeatureRequired = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintStarshipWeapon> WeaponBlueprints
	{
		get
		{
			BlueprintReference<BlueprintStarshipWeapon>[] weaponBlueprints = m_WeaponBlueprints;
			return weaponBlueprints;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> TargetAnyFeatureRequired
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] targetAnyFeatureRequired = m_TargetAnyFeatureRequired;
			return targetAnyFeatureRequired;
		}
	}
}
