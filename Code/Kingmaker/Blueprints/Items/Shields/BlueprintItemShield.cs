using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UI.Common;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Shields;

[ComponentName("Items/BlueprintItemShield")]
[TypeId("1f2124463a6609642a16259c2c2db3b2")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemShield : BlueprintItemEquipmentHand
{
	[SerializeField]
	private BlueprintItemWeaponReference m_WeaponComponent;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemArmorReference m_ArmorComponent;

	public override float Weight => ArmorComponent?.Weight ?? base.Weight;

	[CanBeNull]
	public BlueprintItemWeapon WeaponComponent => m_WeaponComponent.Get();

	public BlueprintItemArmor ArmorComponent => m_ArmorComponent.Get();

	public override string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Name))
			{
				return base.Name;
			}
			return ArmorComponent.Name;
		}
	}

	public override string Description
	{
		get
		{
			if (string.IsNullOrEmpty(base.Description))
			{
				return ArmorComponent.Description;
			}
			return base.Description;
		}
	}

	public override ItemsItemType ItemType => ItemsItemType.Shield;

	public override Sprite Icon
	{
		get
		{
			if (!(base.Icon == null))
			{
				return base.Icon;
			}
			return ArmorComponent?.Icon;
		}
	}

	public override string InventoryEquipSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryEquipSound))
			{
				return base.InventoryEquipSound;
			}
			return ArmorComponent.InventoryEquipSound;
		}
	}

	public override string InventoryTakeSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryTakeSound))
			{
				return base.InventoryTakeSound;
			}
			return ArmorComponent.InventoryTakeSound;
		}
	}

	public override string InventoryPutSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryPutSound))
			{
				return base.InventoryPutSound;
			}
			return ArmorComponent.InventoryPutSound;
		}
	}

	public override string SubtypeName
	{
		get
		{
			if (ArmorComponent == null)
			{
				return "";
			}
			return ArmorComponent.SubtypeName;
		}
	}

	public override string SubtypeDescription
	{
		get
		{
			if (ArmorComponent == null)
			{
				return "";
			}
			return ArmorComponent.SubtypeDescription;
		}
	}
}
