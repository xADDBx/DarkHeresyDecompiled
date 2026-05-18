using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public class ItemsCollection : IItemsCollection, IEnumerable<ItemEntity>, IEnumerable, IAreaCRChangedHandler, ISubscriber, IHashable, IOwlPackable, IOwlPackable<ItemsCollection>
{
	public class DoNotRemoveFromSlot : ContextFlag<DoNotRemoveFromSlot>
	{
	}

	public class SuppressEvents : ContextFlag<SuppressEvents>
	{
	}

	[JsonProperty]
	[OwlPackInclude]
	private List<ItemEntity> m_Items = new List<ItemEntity>();

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef m_OwnerRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ItemsCollection",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_Items", typeof(List<ItemEntity>)),
			new FieldInfo("m_OwnerRef", typeof(EntityRef)),
			new FieldInfo("ForceStackable", typeof(bool)),
			new FieldInfo("IsVendorTable", typeof(bool)),
			new FieldInfo("KeepItemsWithZeroCount", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool ForceStackable { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsVendorTable { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool KeepItemsWithZeroCount { get; set; }

	public IEntity Owner => m_OwnerRef.Entity;

	public Entity ConcreteOwner => (Entity)Owner;

	public BaseUnitEntity OwnerUnit => m_OwnerRef.Entity as BaseUnitEntity;

	public bool IsPostLoadExecuted { get; private set; }

	public bool IsPostLoadFixesExecuted { get; private set; }

	public bool IsDisposingNow { get; private set; }

	public float Weight { get; private set; }

	public ReadonlyList<ItemEntity> Items => m_Items;

	public bool IsPlayerInventory => this == Game.Instance.PartySharedInventory.Collection;

	public bool IsSharedStash => this == Game.Instance.Player.SharedStash;

	public bool HasLoot => m_Items.HasItem((ItemEntity i) => i.IsLootable && i.IsAvailable() && (i.HoldingSlot?.CanRemoveItem() ?? true));

	[JsonConstructor]
	private ItemsCollection()
	{
	}

	public ItemsCollection(Entity owner)
	{
		m_OwnerRef = owner?.Ref ?? default(EntityRef);
		IsPostLoadExecuted = true;
		IsPostLoadFixesExecuted = true;
	}

	public IEnumerator<ItemEntity> GetEnumerator()
	{
		return m_Items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(BlueprintItem item, int count = 1)
	{
		int num = 0;
		for (int i = 0; i < m_Items.Count; i++)
		{
			ItemEntity itemEntity = m_Items[i];
			if (itemEntity.Blueprint == item)
			{
				num += itemEntity.Count;
				if (num >= count)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsAny(IList<BlueprintItem> items)
	{
		for (int i = 0; i < m_Items.Count; i++)
		{
			ItemEntity item = m_Items[i];
			if (items.HasItem((BlueprintItem ii) => ii == item.Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public void Insert(ItemEntity item)
	{
		if (item.Collection != null)
		{
			PFLog.Default.Error($"Item {item} already in collection now");
			item.Collection.Remove(item);
		}
		if (m_Items.Contains(item))
		{
			PFLog.Default.Error($"Collection already contains item {item} (owner: {OwnerUnit})");
		}
		else
		{
			m_Items.Add(item);
		}
		item.Collection = this;
		if (IsPlayerInventory || IsSharedStash)
		{
			item.SourceContainer = null;
		}
		item.UpdateSlotIndex();
		if (!item.IsStackable && item.Count > 1)
		{
			int num = item.Count - 1;
			item.DecrementCount(num, force: true);
			for (int i = 0; i < num; i++)
			{
				Add(item.Blueprint)?.UpdateSlotIndex();
			}
		}
	}

	public void Extract(ItemEntity item)
	{
		m_Items.Remove(item);
		item.Collection = null;
		if (!ContextData<DoNotRemoveFromSlot>.Current)
		{
			item.HoldingSlot?.RemoveItem();
		}
		item.UpdateSlotIndex();
	}

	public ItemEntity Add(ItemEntity newItem, bool noAutoMerge = false)
	{
		if (newItem.Blueprint is IBlueprintItemContainer)
		{
			BlueprintItem blueprint = newItem.Blueprint;
			int count = newItem.Count;
			newItem.Dispose();
			ItemEntity itemEntity = ContainerResolver.TryResolveEntity(blueprint, ResolveEquipmentCR(null));
			if (itemEntity == null)
			{
				return null;
			}
			if (count > 1 && (itemEntity.IsStackable || ForceStackable))
			{
				itemEntity.IncrementCount(count - 1, ForceStackable);
			}
			newItem = itemEntity;
		}
		newItem.SetOriginAreaIfNull(Game.Instance.CurrentlyLoadedArea);
		if (IsPlayerInventory)
		{
			MoneyReplacement component = newItem.Blueprint.GetComponent<MoneyReplacement>();
			if (component != null)
			{
				long amount = component.Cost * newItem.Count;
				Game.Instance.Player.GainMoney(amount);
				OnItemAdded(newItem, newItem.Count);
				newItem.Dispose();
				return null;
			}
			newItem.Time = Game.Instance.Player.GameTime;
		}
		int count2 = newItem.Count;
		if (count2 == 0 && !KeepItemsWithZeroCount)
		{
			return newItem;
		}
		if ((newItem.IsStackable || ForceStackable) && !noAutoMerge)
		{
			bool flag = newItem.IsFromVendorSlot();
			foreach (ItemEntity item in m_Items)
			{
				if (((!flag && !item.IsFromVendorSlot()) || newItem.IsFromSameVendorSlot(item)) && item.TryMerge(newItem))
				{
					OnItemAdded(item, count2);
					return item;
				}
			}
		}
		Insert(newItem);
		OnItemAdded(newItem, count2);
		return newItem;
	}

	public void Add(BlueprintItem newBpItem, int count, [CanBeNull] Action<ItemEntity> callback = null, bool noAutoMerge = false, int? equipmentCR = null)
	{
		int? equipmentCR2 = ResolveEquipmentCR(equipmentCR);
		if (newBpItem.IsActuallyStackable || ForceStackable)
		{
			ItemEntity itemEntity = ContainerResolver.TryResolveEntity(newBpItem, equipmentCR2);
			if (itemEntity != null)
			{
				itemEntity.IncrementCount(count - 1, ForceStackable);
				itemEntity = Add(itemEntity, noAutoMerge);
				InvokeAddCallback(callback, itemEntity);
			}
			return;
		}
		while (count-- > 0)
		{
			ItemEntity itemEntity2 = ContainerResolver.TryResolveEntity(newBpItem, equipmentCR2);
			if (itemEntity2 != null)
			{
				ItemEntity item = Add(itemEntity2, noAutoMerge);
				InvokeAddCallback(callback, item);
			}
		}
	}

	public ItemEntity Add(BlueprintItem newBpItem, int? equipmentCR = null)
	{
		ItemEntity itemEntity = ContainerResolver.TryResolveEntity(newBpItem, ResolveEquipmentCR(equipmentCR));
		if (itemEntity == null)
		{
			return null;
		}
		return Add(itemEntity);
	}

	private int? ResolveEquipmentCR(int? equipmentCR)
	{
		if (equipmentCR.HasValue)
		{
			return equipmentCR;
		}
		if (OwnerUnit != null)
		{
			return UnitEquipmentCRHelper.GetEquipmentCR(OwnerUnit);
		}
		return Game.Instance.CurrentlyLoadedArea?.GetCR();
	}

	private static void InvokeAddCallback([CanBeNull] Action<ItemEntity> callback, ItemEntity item)
	{
		if (callback == null)
		{
			return;
		}
		try
		{
			callback(item);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public ItemEntity Remove(ItemEntity item, int? count = null)
	{
		int valueOrDefault = count.GetValueOrDefault();
		if (!count.HasValue)
		{
			valueOrDefault = item.Count;
			count = valueOrDefault;
		}
		ItemEntity itemEntity = item.Split(count.Value);
		if (itemEntity.Collection != null)
		{
			Extract(itemEntity);
		}
		OnItemsRemoved(item, count.Value);
		return itemEntity;
	}

	public void Remove(BlueprintItem bpItem, int count = 1)
	{
		if (IsPlayerInventory)
		{
			MoneyReplacement component = bpItem.GetComponent<MoneyReplacement>();
			if (component != null)
			{
				long amount = component.Cost * count;
				Game.Instance.Player.SpendMoney(amount);
				ItemEntity itemEntity = bpItem.CreateEntity();
				OnItemsRemoved(itemEntity, count);
				itemEntity.Dispose();
				return;
			}
		}
		int num = count;
		for (int num2 = m_Items.Count - 1; num2 >= 0; num2--)
		{
			ItemEntity itemEntity2 = m_Items[num2];
			if (itemEntity2.Blueprint == bpItem)
			{
				ItemSlot holdingSlot = itemEntity2.HoldingSlot;
				if (holdingSlot != null && !holdingSlot.RemoveItem())
				{
					PFLog.Default.Error("Can't remove {0} of {1}: item equipped and non-removable", num, bpItem);
				}
				else
				{
					int num3 = Math.Min(count, itemEntity2.Count);
					Remove(itemEntity2, num3);
					count -= num3;
					if (count < 1)
					{
						break;
					}
				}
			}
		}
		if (count > 0)
		{
			PFLog.Default.Error("Can't remove {0} of {1} (removed only {2})", num, bpItem, num - count);
		}
	}

	public void RemoveAll()
	{
		for (int num = m_Items.Count - 1; num >= 0; num--)
		{
			ItemEntity item = m_Items[num];
			Remove(item);
		}
	}

	public int RemoveAll([NotNull] BlueprintItem blueprint)
	{
		int num = 0;
		for (int num2 = m_Items.Count - 1; num2 >= 0; num2--)
		{
			ItemEntity itemEntity = m_Items[num2];
			if (itemEntity?.Blueprint == blueprint)
			{
				num += itemEntity.Count;
				Remove(itemEntity);
			}
		}
		return num;
	}

	public ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to)
	{
		return Transfer(item, count, to, noAutoMerge: false);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, ItemsCollection to)
	{
		return Transfer(item, count, to, noAutoMerge: true);
	}

	public ItemEntity Transfer(ItemEntity item, ItemsCollection to)
	{
		return Transfer(item, item.Count, to);
	}

	private ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to, bool noAutoMerge)
	{
		if (!ContextData<DoNotRemoveFromSlot>.Current)
		{
			ItemSlot holdingSlot = item.HoldingSlot;
			if (holdingSlot != null && !holdingSlot.CanRemoveItem())
			{
				return null;
			}
		}
		ItemEntity newItem = Remove(item, count);
		return to.Add(newItem, noAutoMerge);
	}

	protected virtual void OnItemAdded(ItemEntity item, int count)
	{
		if (!ContextData<SuppressEvents>.Current)
		{
			EventBus.RaiseEvent(delegate(IItemsCollectionHandler l)
			{
				l.HandleItemsAdded(this, item, count);
			});
		}
		if (IsPlayerInventory)
		{
			item.TryIdentify();
		}
	}

	protected virtual void OnItemsRemoved(ItemEntity item, int count)
	{
		if (!ContextData<SuppressEvents>.Current)
		{
			EventBus.RaiseEvent(delegate(IItemsCollectionHandler l)
			{
				l.HandleItemsRemoved(this, item, count);
			});
		}
	}

	public void ValidateInventorySlotIndices()
	{
		List<ItemEntity> list = new List<ItemEntity>();
		list.AddRange(Enumerable.Repeat<ItemEntity>(null, Items.Count));
		int num = 0;
		foreach (ItemEntity item in Items)
		{
			if (item.InventorySlotIndex >= 0)
			{
				int inventorySlotIndex = item.InventorySlotIndex;
				if (inventorySlotIndex > num)
				{
					num = inventorySlotIndex;
				}
				if (list.Count <= inventorySlotIndex)
				{
					list.AddRange(Enumerable.Repeat<ItemEntity>(null, num + 1));
				}
				if (list[inventorySlotIndex] != null)
				{
					PFLog.Default.Error("More than one items with index {0} in items collection", inventorySlotIndex);
				}
				list[inventorySlotIndex] = item;
			}
		}
	}

	public void PreSave()
	{
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				item.PreSave();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void PrePostLoad()
	{
		foreach (ItemEntity item in m_Items)
		{
			item.Collection = this;
			item.PrePostLoad();
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted)
		{
			return;
		}
		ValidateInventoryItems();
		bool flag = IsPlayerInventory || this == Game.Instance.Player.SharedStash;
		if (flag)
		{
			m_Items = m_Items.Distinct().ToList();
		}
		List<ItemEntity> list = null;
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				if (flag)
				{
					item.UpdateSlotIndex();
				}
				if ((item.Count < 1 && !KeepItemsWithZeroCount) || item.Collection != this)
				{
					if (list == null)
					{
						list = new List<ItemEntity>();
					}
					list.Add(item);
				}
				if (!item.IsPostLoadExecuted)
				{
					item.PostLoad();
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		if (list != null)
		{
			foreach (ItemEntity item2 in list)
			{
				PFLog.Default.Error($"Remove invalid item: {item2}");
				if (item2.Collection == this)
				{
					Remove(item2);
				}
				else
				{
					m_Items.Remove(item2);
				}
				if (item2.HoldingSlot != null)
				{
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						PFLog.Default.Error($"Remove invalid item from slot: {item2}");
						item2.HoldingSlot.RemoveItem();
					}
				}
			}
		}
		if (flag)
		{
			foreach (ItemEntity item3 in m_Items)
			{
				if (item3.InventorySlotIndex < 0)
				{
					continue;
				}
				foreach (ItemEntity item4 in m_Items)
				{
					if (item3 != item4 && item3.InventorySlotIndex == item4.InventorySlotIndex)
					{
						PFLog.Default.Error($"Restore item's inventory slot index: {item3}");
						item3.UpdateSlotIndex(force: true);
						break;
					}
				}
			}
		}
		IsPostLoadExecuted = true;
	}

	public void ApplyPostLoadFixes()
	{
		if (IsPostLoadFixesExecuted)
		{
			return;
		}
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				item.ApplyPostLoadFixes();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		IsPostLoadFixesExecuted = true;
	}

	private void ValidateInventoryItems()
	{
		using PooledHashSet<ItemEntity> pooledHashSet = PooledHashSet<ItemEntity>.Get(m_Items);
		using PooledHashSet<ItemEntity> pooledHashSet2 = PooledHashSet<ItemEntity>.Get();
		StringBuilder stringBuilder = null;
		for (int num = pooledHashSet.Count - 1; num >= 0; num--)
		{
			ItemEntity itemEntity = m_Items[num];
			if (itemEntity.HoldingSlot != null && itemEntity.HoldingSlot.Owner == null)
			{
				m_Items.RemoveAt(num);
				pooledHashSet2.Add(itemEntity);
				stringBuilder = stringBuilder ?? new StringBuilder();
				stringBuilder.AppendLine($"{itemEntity} was removed. Reason: item.HoldingSlot != null && x.HoldingSlot.Owner == null");
			}
			if (!itemEntity.OnPostLoadValidation())
			{
				m_Items.RemoveAt(num);
				pooledHashSet2.Add(itemEntity);
				stringBuilder = stringBuilder ?? new StringBuilder();
				stringBuilder.AppendLine($"{itemEntity} was removed. Reason: invalid Blueprint type or unexist blueprint");
			}
		}
		if (pooledHashSet2.Count > 0)
		{
			stringBuilder.Insert(0, "ItemEntity.PostLoad: Collection(owner_id: " + m_OwnerRef.Id + ") contains invalid items. Removed items: \n");
			PFLog.Default.Error(stringBuilder.ToString());
		}
	}

	public void Dispose()
	{
		IsDisposingNow = true;
		try
		{
			foreach (ItemEntity item in m_Items)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		finally
		{
			IsDisposingNow = false;
		}
	}

	public bool DropItem(ItemEntity item)
	{
		if (item.Collection != this)
		{
			return false;
		}
		IAbstractUnitEntity player = Game.Instance.Player.MainCharacter.Entity;
		DroppedLootView droppedLootView = UnityEngine.Object.FindObjectsOfType<DroppedLootView>().FirstOrDefault((DroppedLootView o) => o.IsDroppedByPlayer && player.DistanceTo(o.ViewTransform.position) < 5.Feet().Meters);
		if (!droppedLootView)
		{
			droppedLootView = Game.Instance.Controllers.EntitySpawner.SpawnEntityWithView(ConfigRoot.Instance.Prefabs.DroppedLootBag, player.Position, player.Rotation, Game.Instance.State.LoadedAreaState.MainState);
			droppedLootView.Loot = new ItemsCollection(droppedLootView.Data);
			droppedLootView.DroppedBy = (BaseUnitEntity)player;
		}
		Transfer(item, droppedLootView.Loot);
		return true;
	}

	public void Subscribe()
	{
		EventBus.Subscribe(this);
		foreach (ItemEntity item in m_Items)
		{
			item.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		EventBus.Unsubscribe(this);
		foreach (ItemEntity item in m_Items)
		{
			item.Unsubscribe();
		}
	}

	void IAreaCRChangedHandler.HandleAreaCRChanged()
	{
		ReResolveContainerItems();
	}

	private void ReResolveContainerItems()
	{
		using (ContextData<SuppressEvents>.Request())
		{
			int? equipmentCR = ResolveEquipmentCR(null);
			for (int num = m_Items.Count - 1; num >= 0; num--)
			{
				ReResolveContainerItem(m_Items[num], equipmentCR);
			}
		}
	}

	private void ReResolveContainerItem(ItemEntity item, int? equipmentCR)
	{
		ItemEntity itemEntity = ContainerResolver.ReResolve(item, equipmentCR);
		if (itemEntity != null && itemEntity != item)
		{
			ItemSlot holdingSlot = item.HoldingSlot;
			Extract(item);
			item.Dispose();
			Insert(itemEntity);
			holdingSlot?.InsertItem(itemEntity);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<ItemEntity> items = m_Items;
		if (items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				Hash128 val = ClassHasher<ItemEntity>.GetHash128(items[i]);
				result.Append(ref val);
			}
		}
		EntityRef obj = m_OwnerRef;
		Hash128 val2 = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val2);
		bool val3 = ForceStackable;
		result.Append(ref val3);
		bool val4 = IsVendorTable;
		result.Append(ref val4);
		bool val5 = KeepItemsWithZeroCount;
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ItemsCollection source = new ItemsCollection();
		result = Unsafe.As<ItemsCollection, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ItemsCollection>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Items", ref m_Items, state);
		formatter.Field(1, "m_OwnerRef", ref m_OwnerRef, state);
		bool value = ForceStackable;
		formatter.UnmanagedField(2, "ForceStackable", ref value, state);
		bool value2 = IsVendorTable;
		formatter.UnmanagedField(3, "IsVendorTable", ref value2, state);
		bool value3 = KeepItemsWithZeroCount;
		formatter.UnmanagedField(4, "KeepItemsWithZeroCount", ref value3, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemsCollection>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_Items = formatter.ReadPackable<List<ItemEntity>>(state);
				break;
			case 1:
				m_OwnerRef = formatter.ReadPackable<EntityRef>(state);
				break;
			case 2:
				ForceStackable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				IsVendorTable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				KeepItemsWithZeroCount = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
