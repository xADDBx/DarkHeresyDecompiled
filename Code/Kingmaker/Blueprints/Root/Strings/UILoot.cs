using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UILoot
{
	public LocalizedString Loot;

	public LocalizedString CharactersBox;

	public LocalizedString LootPlayerChest;

	public LocalizedString LootSharedStash;

	public LocalizedString SendToPlayerChest;

	public LocalizedString OneSlotLoot;

	public LocalizedString LootOnArea;

	public LocalizedString CollectAll;

	public LocalizedString LeaveZone;

	public LocalizedString CollectAllAndLeaveZone;

	public LocalizedString LootManager;

	public LocalizedString SendToInventory;

	public LocalizedString SendAllToInventory;

	public LocalizedString ItemsLootObject;

	public LocalizedString ItemsLootObjectDescr;

	public LocalizedString CollectAllBeforeLeave;

	public LocalizedString Collect;

	public LocalizedString Inspect;

	public LocalizedString TrashMode;

	public LocalizedString SuitableItems;

	public LocalizedString NoSuitableItems;

	public LocalizedString NotificationMessageForGainLootItem;

	public LocalizedString SkillCheckTitle;

	public LocalizedString SkillCheckResult;

	public LocalizedString SkillCheckValueAgainst;

	public LocalizedString SkillCheckSkillValue;

	public LocalizedString DropZoneUnsupportedItem;

	public LocalizedString LootLockedState;

	public LocalizedString ExitDescription;

	public (string, string) GetLootObjectStrings(LootObjectType objectType)
	{
		if (objectType == LootObjectType.Normal)
		{
			return (ItemsLootObject, ItemsLootObjectDescr);
		}
		return (string.Empty, string.Empty);
	}

	public string GetLootName(LootContainerType type)
	{
		switch (type)
		{
		case LootContainerType.DefaultLoot:
		case LootContainerType.Environment:
		case LootContainerType.Chest:
		case LootContainerType.Unit:
			return Loot;
		case LootContainerType.PlayerChest:
			return CharactersBox;
		case LootContainerType.OneSlot:
			return OneSlotLoot;
		default:
			return Loot;
		}
	}

	public string GetLootNameByContext(LootWindowMode mode)
	{
		switch (mode)
		{
		case LootWindowMode.Short:
		case LootWindowMode.ShortUnit:
		case LootWindowMode.StandardChest:
			return Loot;
		case LootWindowMode.PlayerChest:
			return LootPlayerChest;
		case LootWindowMode.ZoneExit:
			return LootOnArea;
		default:
			return Loot;
		}
	}
}
