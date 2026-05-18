using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Gameplay.Blueprints.Root;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[ComponentName("Items/BlueprintItemArmor")]
[TypeId("579ddce9e6b6d8e44b05e0715cc66741")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemArmor : BlueprintItemEquipment
{
	public ArmorProgressionType ProgressionType;

	public ItemPowerLevel PowerLevel;

	public ItemFaction Faction;

	[SerializeField]
	private bool m_OverrideDamageReduction;

	[SerializeField]
	[ShowIf("OverrideDamageReduction")]
	private int m_DamageReduction;

	[SerializeField]
	private bool m_OverrideDurability;

	[SerializeField]
	[ShowIf("OverrideDurability")]
	private int m_Durability;

	[SerializeField]
	private ArmorVisualParameters m_VisualParameters;

	[SerializeField]
	private WarhammerArmorCategory m_Category;

	[SerializeField]
	[ShowIf("IsHeavyCategory")]
	private int m_MaxDefence;

	private static BlueprintItemProgressionRoot Progression => ConfigRoot.Instance.ItemProgressionRoot;

	private bool IsHeavyCategory => m_Category == WarhammerArmorCategory.Heavy;

	private bool OverrideDamageReduction
	{
		get
		{
			if (!m_OverrideDamageReduction)
			{
				return !Progression.EnableForArmor;
			}
			return true;
		}
	}

	private bool OverrideDurability
	{
		get
		{
			if (!m_OverrideDurability)
			{
				return !Progression.EnableForArmor;
			}
			return true;
		}
	}

	public override string SubtypeName => string.Empty;

	public override ItemsItemType ItemType => ItemsItemType.Armor;

	public override Sprite Icon => base.Icon ?? ConfigRoot.Instance.UIConfig.UIIcons.DefaultIcons.DefaultItemIcon;

	public ArmorVisualParameters VisualParameters => m_VisualParameters;

	public ArmorProficiencyGroup ProficiencyGroup => ArmorProficiencyGroup.None;

	public WarhammerArmorCategory Category => m_Category;

	public int MaxDefence
	{
		get
		{
			if (!IsHeavyCategory)
			{
				return 0;
			}
			return m_MaxDefence;
		}
	}

	public override string InventoryEquipSound
	{
		get
		{
			return VisualParameters.InventoryEquipSound;
		}
		set
		{
			VisualParameters.InventoryEquipSound = value;
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
			return VisualParameters.InventoryPutSound;
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
			return VisualParameters.InventoryTakeSound;
		}
	}

	public IEnumerable<ArmourTagUISettings> ArmourTags => from f in base.ComponentsArray.OfType<AddFactToEquipmentWielder>()
		select f.Fact.GetComponent<ArmourTagUISettings>() into tag
		where tag != null
		select tag;

	public override float Weight => m_Weight;

	public int GetArmorDamageReduction(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideDamageReduction)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).DamageReduction;
		}
		return m_DamageReduction;
	}

	public int GetArmorDurability(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideDurability)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).Durability;
		}
		return m_Durability;
	}

	internal ItemPowerLevel ResolvePowerLevel(ItemPowerLevel powerLevel)
	{
		if (powerLevel == ItemPowerLevel.Undefined)
		{
			return PowerLevel;
		}
		return powerLevel;
	}
}
