using System;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("cdd895d29744d0e46a701718aa9c7f29")]
public class ItemTransfer : GameAction
{
	[SerializeReference]
	public ItemsCollectionEvaluator? Source;

	[SerializeReference]
	public ItemsCollectionEvaluator? Destination;

	public bool TransferAll;

	[HideIf("TransferAll")]
	public LootEntry[] Loot = new LootEntry[0];

	[Tooltip("Don't record this transfer in game log")]
	public bool Silent;

	public override string GetCaption()
	{
		return $"Transfer items from {Source} to {Destination}";
	}

	protected override void RunAction()
	{
		if (Source == null || !Source.TryGetValue(out var value) || value == null)
		{
			throw new Exception("Failed to get item source");
		}
		if (Destination == null || !Destination.TryGetValue(out var value2) || value2 == null)
		{
			throw new Exception("Failed to get item destination");
		}
		using (ContextData<ItemsCollection.SuppressEvents>.RequestIf(Silent))
		{
			if (TransferAll)
			{
				for (int num = value.Items.Count - 1; num >= 0; num--)
				{
					ItemEntity itemEntity = value.Items[num];
					if (itemEntity != null)
					{
						value.Transfer(itemEntity, value2);
					}
				}
			}
			else
			{
				TransferByLootEntries(value, value2);
			}
		}
	}

	private void TransferByLootEntries(ItemsCollection source, ItemsCollection destination)
	{
		LootEntry[] loot = Loot;
		foreach (LootEntry lootEntry in loot)
		{
			int num = lootEntry.Count;
			int num2 = source.Items.Count - 1;
			while (num2 >= 0 && num > 0)
			{
				ItemEntity itemEntity = source.Items[num2];
				if (itemEntity.Blueprint == lootEntry.Item)
				{
					int num3 = Math.Min(itemEntity.Count, num);
					source.Transfer(itemEntity, num3, destination);
					num -= num3;
				}
				num2--;
			}
		}
	}
}
