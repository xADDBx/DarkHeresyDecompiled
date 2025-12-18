using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("4b7adb2fefe3ae14a992780a233a7522")]
public class BlueprintItemArsenal : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemArsenal>
	{
	}

	[Tooltip("Adds altertanive firing mode to this weapon type using provided weapon")]
	public StarshipWeaponType AppliedWeaponType;

	[Tooltip("Filter weapon ammo damage type, None to to ignore filter")]
	public DamageType FilterDamageType = DamageType.None;

	[SerializeField]
	[Tooltip("Weapon variant for alternanive fire mode, weapon blueprint Allowed slots determines where it can appear.\nCan be NULL when only ammo variant needed")]
	private BlueprintStarshipWeapon.Reference m_VariantWeapon;

	[SerializeField]
	[Tooltip("Variant Ammo for altertanive mode.\nCan be NULL to use variant weapon default ammo")]
	private BlueprintStarshipAmmo.Reference m_VariantAmmo;

	public BlueprintStarshipWeapon VariantWeapon => m_VariantWeapon?.Get();

	public BlueprintStarshipAmmo VariantAmmo => m_VariantAmmo?.Get();

	public override ItemsItemType ItemType => ItemsItemType.StarshipArsenal;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
