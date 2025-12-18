using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("5f66ee2522e41c842be4bfe85a2a61ac")]
public sealed class TrashLootSettings : BlueprintScriptableObject, IBlueprintScanner
{
	[Serializable]
	public class TypeSettings
	{
		public ItemsItemOrigin Type;

		public List<BlueprintItemReference> Items = new List<BlueprintItemReference>();
	}

	[Serializable]
	public class TypeChance
	{
		[Serializable]
		public class ItemData
		{
			public int Weight;

			public ItemsItemOrigin Type;

			public ItemQuality ItemQuality;
		}

		public LootSetting Setting;

		public ItemData[] Types = new ItemData[0];
	}

	[Serializable]
	public class TrashLootCostEntry
	{
		public TrashLootAmount Amount;

		public int Cost;
	}

	[Serializable]
	public class TotalCostToSlotsCount
	{
		public int Cost;

		public int SlotsCount;
	}

	[Serializable]
	public class ItemCostToItemQuality
	{
		public int Cost;

		public ItemQuality Quality;
	}

	private static TrashLootSettings s_MainInstance;

	public TrashLootCostEntry[] TrashLootAmountTable = new TrashLootCostEntry[0];

	public float[] CRToCostFactor = new float[1] { 1f };

	public int[] ChanceToIncreaseItemsCount = new int[5] { 75, 50, 30, 15, 5 };

	public TotalCostToSlotsCount[] TotalCostToSlotsCountTable = new TotalCostToSlotsCount[5]
	{
		new TotalCostToSlotsCount
		{
			Cost = 0,
			SlotsCount = 1
		},
		new TotalCostToSlotsCount
		{
			Cost = 500,
			SlotsCount = 2
		},
		new TotalCostToSlotsCount
		{
			Cost = 1000,
			SlotsCount = 3
		},
		new TotalCostToSlotsCount
		{
			Cost = 2000,
			SlotsCount = 4
		},
		new TotalCostToSlotsCount
		{
			Cost = 3000,
			SlotsCount = 5
		}
	};

	public ItemCostToItemQuality[] ItemCostToItemQualityTable = new ItemCostToItemQuality[3]
	{
		new ItemCostToItemQuality
		{
			Cost = 0,
			Quality = ItemQuality.Cheap
		},
		new ItemCostToItemQuality
		{
			Cost = 100,
			Quality = ItemQuality.Middle
		},
		new ItemCostToItemQuality
		{
			Cost = 500,
			Quality = ItemQuality.Expensive
		}
	};

	public List<TypeSettings> Types = new List<TypeSettings>();

	public TypeChance[] Table = new TypeChance[0];

	public void Fill(BlueprintLoot target)
	{
		if (target.Type != 0)
		{
			PFLog.Default.Error($"Invalid loot type {target.Type}");
			return;
		}
		int targetCost = GetTargetCost(target.TrashLootAmount, target.Area.GetCR());
		target.Items = RandomizeLoot(target.Setting, targetCost);
	}

	private LootEntry[] RandomizeLoot(LootSetting setting, int targetCost, [CanBeNull] Func<int, int, int> randomNumberGenerator = null)
	{
		if (targetCost <= 0)
		{
			PFLog.Default.Error("Trash loot is not configured");
			return Array.Empty<LootEntry>();
		}
		int slotsCount = GetSlotsCount(targetCost);
		return RandomizeLoot(setting, targetCost, slotsCount, randomNumberGenerator);
	}

	private LootEntry[] RandomizeLoot(LootSetting setting, int targetCost, int slots, [CanBeNull] Func<int, int, int> randomNumberGenerator = null)
	{
		if (randomNumberGenerator == null)
		{
			randomNumberGenerator = PFStatefulRandom.Blueprints.Range;
		}
		TypeChance.ItemData[] array = Table.FirstItem((TypeChance i) => i.Setting == setting)?.Types;
		if (array == null)
		{
			return Array.Empty<LootEntry>();
		}
		List<LootEntry> list = new List<LootEntry>();
		int num = targetCost;
		while (slots-- > 0)
		{
			BlueprintItem blueprintItem = null;
			int num2 = 5;
			while (blueprintItem == null && num2-- > 0)
			{
				TypeChance.ItemData itemData = Randomize(array, null, randomNumberGenerator);
				if (itemData != null && TryGetRandomItem(randomNumberGenerator, itemData, num, out var item))
				{
					blueprintItem = item;
				}
			}
			if (blueprintItem == null)
			{
				break;
			}
			LootEntry lootEntry = new LootEntry
			{
				Item = blueprintItem,
				Count = 1
			};
			list.Add(lootEntry);
			num -= blueprintItem.Cost;
			while (num - lootEntry.Item.Cost > 0 && lootEntry.Count <= ChanceToIncreaseItemsCount.Length && !((float)ChanceToIncreaseItemsCount[lootEntry.Count - 1] <= (float)randomNumberGenerator(0, 100)))
			{
				lootEntry.Count++;
				num -= blueprintItem.Cost;
			}
		}
		if (num > 0)
		{
			list.Add(new LootEntry
			{
				Item = ConfigRoot.Instance.SystemMechanics.Money,
				Count = num
			});
		}
		return list.ToArray();
	}

	private bool TryGetRandomItem(Func<int, int, int> randomNumberGenerator, TypeChance.ItemData type, int maxCost, out BlueprintItem item)
	{
		item = null;
		List<BlueprintItemReference> list = Types.FirstItem((TypeSettings t) => t.Type == type.Type)?.Items;
		if (list == null)
		{
			PFLog.Default.Error($"Has no items for type {type}");
			return false;
		}
		item = (from i in list
			select i.Get() into it
			where it.Cost <= maxCost && type.ItemQuality.HasFlag(GetItemQuality(it))
			select it).Random(PFStatefulRandom.Blueprints, randomNumberGenerator);
		return true;
	}

	private static TypeChance.ItemData Randomize(TypeChance.ItemData[] types, ItemsItemOrigin? except, [NotNull] Func<int, int, int> randomFromRange)
	{
		if (types.Length == 0)
		{
			PFLog.Default.Error("Trash loot settings are not configured");
			return null;
		}
		int arg = types.Aggregate(0, (int s, TypeChance.ItemData i) => s + i.Weight);
		int num = randomFromRange(0, arg);
		int num2 = 0;
		foreach (TypeChance.ItemData itemData in types)
		{
			num2 += itemData.Weight;
			if (num2 > num && except != itemData.Type)
			{
				return itemData;
			}
		}
		return types[^1];
	}

	private int GetTargetCost(TrashLootAmount amount, int cr)
	{
		int num = TrashLootAmountTable.FirstOrDefault((TrashLootCostEntry i) => i.Amount == amount)?.Cost ?? 0;
		float num2 = CRToCostFactor.LastOrDefault((float i) => i <= (float)cr, 1f);
		return Mathf.RoundToInt((float)num * num2);
	}

	private int GetSlotsCount(int targetCost)
	{
		return TotalCostToSlotsCountTable.LastOrDefault((TotalCostToSlotsCount i) => i.Cost <= targetCost)?.SlotsCount ?? 1;
	}

	public ItemQuality GetItemQuality(BlueprintItem item)
	{
		return ItemCostToItemQualityTable.LastOrDefault((ItemCostToItemQuality i) => i.Cost <= item.Cost)?.Quality ?? ItemQuality.Cheap;
	}

	public void Scan()
	{
	}
}
