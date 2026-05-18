using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Features.Items.Utility;

public class QuickSlotsReplenishLogic : IDisposable, ITurnBasedModeHandler, ISubscriber, IAreaHandler
{
	private readonly IDisposable m_EventsSubscription;

	private readonly int m_QuickSlotsCount;

	private readonly Dictionary<UsableSlot, BlueprintItem> m_CachedItems = new Dictionary<UsableSlot, BlueprintItem>();

	public QuickSlotsReplenishLogic(int quickSlotsCount = 2)
	{
		m_QuickSlotsCount = quickSlotsCount;
		m_EventsSubscription = EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		m_EventsSubscription.Dispose();
		m_CachedItems.Clear();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			List<BaseUnitEntity> actualGroup = Game.Instance.Controllers.SelectionCharacter.ActualGroup;
			CacheQuickSlotItems(actualGroup);
		}
		else
		{
			ReplenishQuickSlotItems();
		}
	}

	public void OnAreaDidLoad()
	{
		if (Game.Instance.Controllers.TurnController.InCombat)
		{
			List<BaseUnitEntity> actualGroup = Game.Instance.Controllers.SelectionCharacter.ActualGroup;
			CacheQuickSlotItems(actualGroup);
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	private void CacheQuickSlotItems(IEnumerable<BaseUnitEntity> units)
	{
		foreach (BaseUnitEntity unit in units)
		{
			CacheUnitQuickSlotsItems(unit);
		}
	}

	private void ReplenishQuickSlotItems()
	{
		ItemsCollection collection = Game.Instance.PartySharedInventory.Collection;
		Dictionary<MechanicEntity, List<BlueprintItem>> dictionary = new Dictionary<MechanicEntity, List<BlueprintItem>>();
		Dictionary<MechanicEntity, List<BlueprintItem>> dictionary2 = new Dictionary<MechanicEntity, List<BlueprintItem>>();
		foreach (KeyValuePair<UsableSlot, BlueprintItem> cachedItem in m_CachedItems)
		{
			var (usableSlot2, blueprintItem) = (KeyValuePair<UsableSlot, BlueprintItem>)(ref cachedItem);
			if (!usableSlot2.HasItem && blueprintItem != null)
			{
				ItemEntity itemEntity = collection.FirstOrDefault((ItemEntity i) => i.Blueprint == blueprintItem && i.Wielder == null);
				if (itemEntity == null || !usableSlot2.CanInsertItem(itemEntity))
				{
					GetItemsList(usableSlot2.Owner, dictionary2).Add(blueprintItem);
					continue;
				}
				usableSlot2.InsertItem(itemEntity);
				GetItemsList(usableSlot2.Owner, dictionary).Add(blueprintItem);
			}
		}
		m_CachedItems.Clear();
		QuickSlotsReplenishResult result = new QuickSlotsReplenishResult
		{
			ReplenishSuccess = dictionary,
			ReplenishFailure = dictionary2
		};
		EventBus.RaiseEvent(delegate(IPartyQuickSlotsReplenishedHandler h)
		{
			h.HandleQuickSlotsReplenished(result);
		});
		PlayReplenishResultSound(dictionary.Count, dictionary2.Count);
		static List<BlueprintItem> GetItemsList(MechanicEntity entity, Dictionary<MechanicEntity, List<BlueprintItem>> dict)
		{
			if (!dict.TryGetValue(entity, out var value))
			{
				value = new List<BlueprintItem>();
				dict.Add(entity, value);
			}
			return value;
		}
	}

	private void CacheUnitQuickSlotsItems(PartUnitBody.IOwner unit)
	{
		UsableSlot[] quickSlots = unit.Body.QuickSlots;
		int num = Mathf.Min(m_QuickSlotsCount, quickSlots.Length);
		for (int i = 0; i < num; i++)
		{
			UsableSlot usableSlot = quickSlots[i];
			if (usableSlot.HasItem)
			{
				ItemEntityUsable item = quickSlots[i].Item;
				m_CachedItems[usableSlot] = item.Blueprint;
			}
		}
	}

	private void PlayReplenishResultSound(int successCount, int failCount)
	{
		bool num = successCount > 0 && failCount < 1;
		bool flag = failCount > 0;
		if (num)
		{
			CombatSounds.Instance.QuickSlotsReplenishSounds.Success.Play();
		}
		else if (flag)
		{
			CombatSounds.Instance.QuickSlotsReplenishSounds.Failure.Play();
		}
	}
}
