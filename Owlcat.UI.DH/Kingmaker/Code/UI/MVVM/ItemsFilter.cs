using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.Code.UI.MVVM;

public static class ItemsFilter
{
	private static readonly Dictionary<ItemEntity, ItemTooltipData> s_ItemTooltipDataSet = new Dictionary<ItemEntity, ItemTooltipData>();

	public static bool ShouldShowItem(ItemEntity item, ItemsFilterType filter)
	{
		return ShouldShowItem(item.Blueprint, filter, item);
	}

	public static bool ShouldShowItem(BlueprintItem blueprintItem, ItemsFilterType filter, ItemEntity item = null)
	{
		switch (filter)
		{
		case ItemsFilterType.NoFilter:
			return true;
		case ItemsFilterType.Weapon:
			return blueprintItem is BlueprintItemWeapon;
		case ItemsFilterType.Armor:
			return blueprintItem is BlueprintItemArmor || blueprintItem is BlueprintItemShield;
		case ItemsFilterType.Accessories:
			return blueprintItem is BlueprintItemEquipmentNeck || blueprintItem is BlueprintItemEquipmentRing || blueprintItem is BlueprintItemEquipmentWrist || blueprintItem is BlueprintItemEquipmentBelt || blueprintItem is BlueprintItemEquipmentShoulders || blueprintItem is BlueprintItemEquipmentGloves || blueprintItem is BlueprintItemEquipmentFeet || blueprintItem is BlueprintItemEquipmentHead || blueprintItem is BlueprintItemEquipmentGlasses || blueprintItem is BlueprintItemEquipmentShirt;
		case ItemsFilterType.Usable:
			return blueprintItem is BlueprintItemEquipmentUsable || blueprintItem.Tag != ItemTag.None;
		case ItemsFilterType.Notable:
			return blueprintItem.IsNotable || blueprintItem.Rarity == BlueprintItem.ItemRarity.Quest;
		case ItemsFilterType.NonUsable:
			return !blueprintItem.IsNotable && !(blueprintItem is BlueprintItemEquipmentUsable) && !(blueprintItem is BlueprintItemWeapon) && !(blueprintItem is BlueprintItemArmor) && !(blueprintItem is BlueprintItemShield) && !(blueprintItem is BlueprintItemEquipmentNeck) && !(blueprintItem is BlueprintItemEquipmentRing) && !(blueprintItem is BlueprintItemEquipmentShoulders) && !(blueprintItem is BlueprintItemEquipmentGloves) && !(blueprintItem is BlueprintItemEquipmentFeet) && !(blueprintItem is BlueprintItemEquipmentHead) && !(blueprintItem is BlueprintItemEquipmentGlasses) && !(blueprintItem is BlueprintItemEquipmentWrist) && !(blueprintItem is BlueprintItemEquipmentShirt) && !(blueprintItem is BlueprintItemEquipmentBelt);
		case ItemsFilterType.ShipNoFilter:
		case ItemsFilterType.ShipWeapon:
		case ItemsFilterType.ShipOther:
		case ItemsFilterType.PlasmaDrives:
		case ItemsFilterType.VoidShieldGenerator:
		case ItemsFilterType.AugerArray:
		case ItemsFilterType.ArmorPlating:
		case ItemsFilterType.Dorsal:
		case ItemsFilterType.Prow:
		case ItemsFilterType.Port:
		case ItemsFilterType.Starboard:
		case ItemsFilterType.Arsenal:
			return false;
		case ItemsFilterType.Trash:
			return blueprintItem.IsTrash();
		case ItemsFilterType.BuyBack:
			return item != null && item.IsSoldByPlayer() && (VendorHelper.TradeLogic.ShowSoldItemsToVendorFilter.Value || !item.IsTrash());
		default:
			throw new ArgumentOutOfRangeException("filter", filter, null);
		}
	}

	public static bool IsMatchSearchRequest(ItemEntity item, string searchRequest)
	{
		if (string.IsNullOrEmpty(searchRequest))
		{
			return true;
		}
		string searchLower = searchRequest.ToLowerInvariant();
		if (item.Name.ToLowerInvariant().Contains(searchLower))
		{
			return true;
		}
		foreach (ItemsFilterType value3 in Enum.GetValues(typeof(ItemsFilterType)))
		{
			if (LocalizedTexts.Instance.ItemsFilter.GetText(value3).Equals(searchRequest, StringComparison.InvariantCultureIgnoreCase))
			{
				return ShouldShowItem(item, value3);
			}
		}
		if (!s_ItemTooltipDataSet.TryGetValue(item, out var value))
		{
			value = TooltipsDataCache.Instance.GetItemTooltipData(item);
			s_ItemTooltipDataSet[item] = value;
		}
		foreach (KeyValuePair<TooltipElement, string> text in value.Texts)
		{
			TooltipElement key = text.Key;
			if (key != 0 && key != TooltipElement.Count && key != TooltipElement.Wielder && key != TooltipElement.WielderSlot && key != TooltipElement.Charges && key != TooltipElement.ItemType && key != TooltipElement.Price && key != TooltipElement.SellPrice && key != TooltipElement.Damage && key != TooltipElement.EquipDamage && key != TooltipElement.PhysicalDamage && key != TooltipElement.CasterLevel)
			{
				string value2 = text.Value;
				if (value2 != null && value2.ToLowerInvariant().Contains(searchLower))
				{
					return true;
				}
			}
		}
		return value.OtherDamage.Any((KeyValuePair<DamageType, string> d) => UIUtilityText.GetTextByKey(d.Key).ToLowerInvariant().Contains(searchLower));
	}

	public static Comparison<ItemEntity> GetItemsDefaultComparison(ItemsSorterType type, ItemsFilterType filter)
	{
		return type switch
		{
			ItemsSorterType.TypeUp => (ItemEntity a, ItemEntity b) => CompareByType(a, b, filter), 
			ItemsSorterType.TypeDown => (ItemEntity a, ItemEntity b) => -CompareByType(a, b, filter), 
			ItemsSorterType.CharacteristicsUp => (ItemEntity a, ItemEntity b) => CompareByCharacteristic(a, b, filter), 
			ItemsSorterType.CharacteristicsDown => (ItemEntity a, ItemEntity b) => -CompareByCharacteristic(a, b, filter), 
			ItemsSorterType.DateUp => (ItemEntity a, ItemEntity b) => CompareByDate(a, b, filter), 
			ItemsSorterType.DateDown => (ItemEntity a, ItemEntity b) => -CompareByDate(a, b, filter), 
			ItemsSorterType.NameUp => (ItemEntity a, ItemEntity b) => CompareByName(a, b, filter), 
			ItemsSorterType.NameDown => (ItemEntity a, ItemEntity b) => -CompareByName(a, b, filter), 
			_ => null, 
		};
	}

	private static int CompareByDate(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int num = a.Time.CompareTo(b.Time);
		if (num != 0)
		{
			return num;
		}
		ItemsItemType itemType = GetItemType(a, filter);
		ItemsItemType itemType2 = GetItemType(b, filter);
		num = itemType.CompareTo(itemType2);
		if (num == 0)
		{
			return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
		}
		return num;
	}

	private static int CompareByType(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int typeCompareValue = TypeSorter.GetTypeCompareValue(a, filter);
		int typeCompareValue2 = TypeSorter.GetTypeCompareValue(b, filter);
		return typeCompareValue.CompareTo(typeCompareValue2);
	}

	private static int CompareByCharacteristic(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int defaultTypeCompareValue = CharacteristicSorter.GetDefaultTypeCompareValue(a, filter);
		int defaultTypeCompareValue2 = CharacteristicSorter.GetDefaultTypeCompareValue(b, filter);
		return defaultTypeCompareValue.CompareTo(defaultTypeCompareValue2);
	}

	private static int CompareByName(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		ItemsItemType itemType = GetItemType(a, filter);
		ItemsItemType itemType2 = GetItemType(b, filter);
		int num = itemType.CompareTo(itemType2);
		if (num == 0)
		{
			return string.Compare(a.Name.TrimStart('['), b.Name.TrimStart('['), StringComparison.Ordinal);
		}
		return num;
	}

	public static ItemsItemType GetItemType(ItemEntity item, ItemsFilterType filter = ItemsFilterType.NoFilter)
	{
		return GetItemType(item.Blueprint, filter);
	}

	public static ItemsItemType GetItemType(BlueprintItem item, ItemsFilterType filter = ItemsFilterType.NoFilter)
	{
		if (!ShouldShowItem(item, filter))
		{
			return ItemsItemType.Other;
		}
		if (!(item is BlueprintItemWeapon))
		{
			if (!(item is BlueprintItemShield))
			{
				if (!(item is BlueprintItemArmor))
				{
					if (!(item is BlueprintItemEquipmentShirt))
					{
						if (!(item is BlueprintItemEquipmentRing))
						{
							if (!(item is BlueprintItemEquipmentBelt))
							{
								if (!(item is BlueprintItemEquipmentFeet))
								{
									if (!(item is BlueprintItemEquipmentGloves))
									{
										if (!(item is BlueprintItemEquipmentHead))
										{
											if (!(item is BlueprintItemEquipmentGlasses))
											{
												if (!(item is BlueprintItemEquipmentNeck))
												{
													if (!(item is BlueprintItemEquipmentShoulders))
													{
														if (!(item is BlueprintItemEquipmentWrist))
														{
															if (item is BlueprintItemEquipmentUsable)
															{
																return ItemsItemType.Usable;
															}
															return ItemsItemType.NonUsable;
														}
														return ItemsItemType.Wrist;
													}
													return ItemsItemType.Shoulders;
												}
												return ItemsItemType.Neck;
											}
											return ItemsItemType.Glasses;
										}
										return ItemsItemType.Head;
									}
									return ItemsItemType.Gloves;
								}
								return ItemsItemType.Feet;
							}
							return ItemsItemType.Belt;
						}
						return ItemsItemType.Ring;
					}
					return ItemsItemType.Shirt;
				}
				return ItemsItemType.Armor;
			}
			return ItemsItemType.Shield;
		}
		return ItemsItemType.Weapon;
	}
}
