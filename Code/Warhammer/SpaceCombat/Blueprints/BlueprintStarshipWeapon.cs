using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("d2513f4af02e4554bb8f9f79a5c35a46")]
public class BlueprintStarshipWeapon : BlueprintStarshipItem
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStarshipWeapon>
	{
	}

	public bool RemoveFromSlotWhenNoCharges;

	public WeaponAbilityContainer WeaponAbilities;

	public StarshipWeaponType WeaponType;

	public WeaponSlotType[] AllowedSlots;

	public int DamageInstances;

	public float DelayBetweenProjectiles;

	[ShowIf("AllowSeries")]
	public int ShotsInSeries = 1;

	[ShowIf("HasSeries")]
	public float DelayInSeries;

	[SerializeField]
	private AkSwitchReference m_SoundTypeSwitch;

	[SerializeField]
	private BlueprintStarshipAmmo.Reference m_DefaultAmmo;

	[SerializeField]
	private BlueprintStarshipAmmo.Reference m_AlternateAmmo;

	public override ItemsItemType ItemType => ItemsItemType.StarshipWeapon;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool AllowSeries => WeaponType == StarshipWeaponType.Macrobatteries;

	public bool HasSeries
	{
		get
		{
			if (AllowSeries)
			{
				return ShotsInSeries > 1;
			}
			return false;
		}
	}

	public override IEnumerable<BlueprintAbility> Abilities => base.Abilities.Concat(WeaponAbilities.Select((WeaponAbility i) => i.Ability).NotNull());

	public override bool GainAbility
	{
		get
		{
			if (!base.GainAbility)
			{
				return WeaponAbilities.Any();
			}
			return true;
		}
	}

	public BlueprintStarshipAmmo DefaultAmmo => m_DefaultAmmo?.Get();

	public BlueprintStarshipAmmo AlternateAmmo => m_AlternateAmmo?.Get();

	public AkSwitchReference SoundTypeSwitch => m_SoundTypeSwitch;
}
