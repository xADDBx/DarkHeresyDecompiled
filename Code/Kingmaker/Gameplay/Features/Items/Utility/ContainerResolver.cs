using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Items;

namespace Kingmaker.Gameplay.Features.Items.Utility;

public static class ContainerResolver
{
	public static ItemEntity TryResolveEntity(BlueprintItem blueprint, int? equipmentCR)
	{
		if (!(blueprint is IBlueprintItemContainer blueprintItemContainer))
		{
			return blueprint.CreateEntity();
		}
		if (!Resolve(blueprintItemContainer, blueprint, equipmentCR, out var concreteItem, out var powerLevel))
		{
			return null;
		}
		return BuildEntity(concreteItem, powerLevel, blueprintItemContainer.OverrideFaction, blueprint);
	}

	public static ItemEntity ReResolve(ItemEntity existing, int? equipmentCR)
	{
		BlueprintItem sourceContainer = existing.SourceContainer;
		if (!(sourceContainer is IBlueprintItemContainer blueprintItemContainer))
		{
			return null;
		}
		if (!Resolve(blueprintItemContainer, sourceContainer, equipmentCR, out var concreteItem, out var powerLevel))
		{
			return null;
		}
		if (concreteItem == existing.Blueprint)
		{
			ApplyOverrides(existing, powerLevel, blueprintItemContainer.OverrideFaction);
			return existing;
		}
		return BuildEntity(concreteItem, powerLevel, blueprintItemContainer.OverrideFaction, sourceContainer);
	}

	private static bool Resolve(IBlueprintItemContainer container, BlueprintItem sourceBp, int? equipmentCR, out BlueprintItem concreteItem, out ItemPowerLevel powerLevel)
	{
		powerLevel = ResolvePowerLevel(container, equipmentCR);
		concreteItem = ResolveConcreteItem(container, powerLevel);
		if (concreteItem != null)
		{
			return true;
		}
		PFLog.Default.Error("Container " + sourceBp.NameSafe() + " has no concrete item to resolve to");
		return false;
	}

	private static ItemEntity BuildEntity(BlueprintItem concreteItem, ItemPowerLevel powerLevel, ItemFaction factionOverride, BlueprintItem sourceContainer)
	{
		ItemEntity itemEntity = concreteItem.CreateEntity();
		ApplyOverrides(itemEntity, powerLevel, factionOverride);
		itemEntity.SourceContainer = sourceContainer;
		return itemEntity;
	}

	private static ItemPowerLevel ResolvePowerLevel(IBlueprintItemContainer container, int? equipmentCR)
	{
		if (!equipmentCR.HasValue)
		{
			return ItemPowerLevel.Undefined;
		}
		ItemPowerLevel itemPowerLevel = CRToPowerLevelTable.Lookup(container.CRToPowerLevelOverride, equipmentCR.Value);
		if (itemPowerLevel != 0)
		{
			return itemPowerLevel;
		}
		return ConfigRoot.Instance.ItemProgressionRoot.GetPowerLevelForCR(equipmentCR.Value);
	}

	private static BlueprintItem ResolveConcreteItem(IBlueprintItemContainer container, ItemPowerLevel powerLevel)
	{
		BlueprintItem blueprintItem = null;
		ItemPowerLevel itemPowerLevel = ItemPowerLevel.Undefined;
		foreach (PowerLevelToItemEntry item in container.PowerLevelToItemOverride)
		{
			if (item.Item != null && item.PowerLevel <= powerLevel && item.PowerLevel > itemPowerLevel)
			{
				itemPowerLevel = item.PowerLevel;
				blueprintItem = item.Item;
			}
		}
		return blueprintItem ?? container.DefaultConcreteItem;
	}

	private static void ApplyOverrides(ItemEntity entity, ItemPowerLevel powerLevel, ItemFaction factionOverride)
	{
		if (!(entity is ItemEntityWeapon itemEntityWeapon))
		{
			if (entity is ItemEntityArmor itemEntityArmor)
			{
				itemEntityArmor.PowerLevelOverride = powerLevel;
				itemEntityArmor.FactionOverride = factionOverride;
				itemEntityArmor.RefreshArmorModifiers();
			}
		}
		else
		{
			itemEntityWeapon.PowerLevelOverride = powerLevel;
			itemEntityWeapon.FactionOverride = factionOverride;
			itemEntityWeapon.Actor.InvalidateAllStatCaches("ApplyOverrides");
		}
	}
}
