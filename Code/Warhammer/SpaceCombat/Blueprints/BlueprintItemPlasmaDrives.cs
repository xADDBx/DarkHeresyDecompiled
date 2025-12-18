using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("ca5f3cfd04654b9489e1b267fc30a044")]
public class BlueprintItemPlasmaDrives : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintItemPlasmaDrives>
	{
	}

	[Header("Propulsion data")]
	public int Speed;

	public int Inertia;

	public int Evasion;

	public int PushPhase;

	public int FinishPhase;

	[Header("Drive explosion")]
	[SerializeField]
	private BlueprintStarshipWeapon.Reference m_ExplosionWeapon;

	[SerializeField]
	private BlueprintStarshipAmmo.Reference m_ExplosionAmmo;

	public BlueprintStarshipWeapon ExplosionWeapon => m_ExplosionWeapon?.Get();

	public BlueprintStarshipAmmo ExplosionAmmo => m_ExplosionAmmo?.Get();

	public override ItemsItemType ItemType => ItemsItemType.StarshipPlasmaDrives;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
