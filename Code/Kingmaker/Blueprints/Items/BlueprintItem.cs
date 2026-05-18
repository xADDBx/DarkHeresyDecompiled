using System;
using Code.Enums;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Items;

[Serializable]
[ComponentName("Items/BlueprintItem")]
[TypeId("bdd0ca0d56a2ac5479e67a3f2bda917f")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItem : BlueprintMechanicEntityFact
{
	public enum ItemRarity
	{
		Trash,
		Lore,
		Common,
		Pattern,
		Refined,
		Unique,
		Quest
	}

	[SerializeField]
	private LocalizedString m_FlavorText;

	[SerializeField]
	private LocalizedString m_NonIdentifiedNameText;

	[SerializeField]
	private LocalizedString m_NonIdentifiedDescriptionText;

	[SerializeField]
	[ShowIf("AllowMakeStackable")]
	private bool m_NotStackable;

	[SerializeField]
	protected float m_Weight;

	[SerializeField]
	private ItemsItemOrigin m_Origin = ItemsItemOrigin.Miscellaneous;

	[SerializeField]
	private ItemRarity m_Rarity = ItemRarity.Common;

	[SerializeField]
	private ItemTag m_Tag;

	[SerializeField]
	private BlueprintItemPatternReference m_Pattern;

	[SerializeField]
	private bool m_IsNotable;

	[SerializeField]
	private int m_Cost;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryPutSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryTakeSound;

	[Obsolete]
	[HideInInspector]
	public bool ToCargoAutomatically;

	public virtual float Weight => m_Weight;

	public ItemsItemOrigin Origin => m_Origin;

	public bool IsNotable => m_IsNotable;

	public int Cost => m_Cost;

	public ItemRarity Rarity => m_Rarity;

	public ItemTag Tag => m_Tag;

	protected virtual bool AllowMakeStackable => true;

	public virtual string FlavorText => m_FlavorText;

	public virtual string SubtypeName => "";

	public virtual string SubtypeDescription => "";

	public virtual ItemsItemType ItemType => ItemsItemType.NonUsable;

	public bool IsActuallyStackable
	{
		get
		{
			if (AllowMakeStackable)
			{
				return !m_NotStackable;
			}
			return false;
		}
	}

	public virtual int IdentifyDC => 0;

	public virtual bool IsLootable => true;

	public virtual string InventoryPutSound
	{
		get
		{
			return m_InventoryPutSound;
		}
		set
		{
			m_InventoryPutSound = value;
		}
	}

	public virtual string InventoryTakeSound
	{
		get
		{
			return m_InventoryTakeSound;
		}
		set
		{
			m_InventoryTakeSound = value;
		}
	}

	public override string ToString()
	{
		return "[" + GetType().Name + ": " + base.ToString() + "]";
	}

	protected override Type GetFactType()
	{
		return GetType();
	}
}
