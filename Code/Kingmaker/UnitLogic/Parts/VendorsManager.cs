using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class VendorsManager : Entity, IChangeChapterHandler, ISubscriber, IHashable, IOwlPackable<VendorsManager>
{
	public const string ID = "vendors-manager-id";

	public new static readonly EntityRef<VendorsManager> Ref = new EntityRef<VendorsManager>("vendors-manager-id");

	public static readonly TimeSpan CleanupStoreItemsFrequency = 1.Weeks();

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintSharedVendorTable, ItemsCollection> _tables = new Dictionary<BlueprintSharedVendorTable, ItemsCollection>();

	[JsonProperty]
	[OwlPackInclude]
	private HashSet<BlueprintItem> _itemsMarkedAsTrash = new HashSet<BlueprintItem>();

	[JsonProperty]
	[OwlPackInclude]
	private List<DetectedVendorData> _detectedVendors = new List<DetectedVendorData>();

	[JsonProperty]
	[OwlPackInclude]
	private ItemsCollection _itemsForBuy = new ItemsCollection(null)
	{
		IsVendorTable = true,
		ForceStackable = true
	};

	[JsonProperty]
	[OwlPackInclude]
	private ItemsCollection _itemsForSell = new ItemsCollection(null)
	{
		IsVendorTable = true,
		ForceStackable = true
	};

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<FactionType, int> _discounts = new Dictionary<FactionType, int>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "VendorsManager",
		OldNames = null,
		Fields = new FieldInfo[16]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("_tables", typeof(Dictionary<BlueprintSharedVendorTable, ItemsCollection>)),
			new FieldInfo("_itemsMarkedAsTrash", typeof(HashSet<BlueprintItem>)),
			new FieldInfo("_detectedVendors", typeof(List<DetectedVendorData>)),
			new FieldInfo("_itemsForBuy", typeof(ItemsCollection)),
			new FieldInfo("_itemsForSell", typeof(ItemsCollection)),
			new FieldInfo("_discounts", typeof(Dictionary<FactionType, int>))
		}
	};

	public override bool NeedsView => false;

	public IEnumerable<DetectedVendorData> DetectedVendors => _detectedVendors;

	public ItemsCollection ItemsForBuy => _itemsForBuy;

	public ItemsCollection ItemsForSell => _itemsForSell;

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	public VendorsManager()
		: base("vendors-manager-id", isInGame: true)
	{
	}

	private VendorsManager(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public bool IsMarkedAsTrash(BlueprintItem item)
	{
		return _itemsMarkedAsTrash.Contains(item);
	}

	public bool IsMarkedAsTrash(ItemEntity item)
	{
		return IsMarkedAsTrash(item.Blueprint);
	}

	public bool IsTrash(BlueprintItem item)
	{
		if (item.Rarity != 0)
		{
			return IsMarkedAsTrash(item);
		}
		return true;
	}

	public bool IsTrash(ItemEntity item)
	{
		return IsTrash(item.Blueprint);
	}

	public void AddToTrash(BlueprintItem item)
	{
		_itemsMarkedAsTrash.Add(item);
	}

	public void AddToTrash(ItemEntity item)
	{
		AddToTrash(item.Blueprint);
	}

	public void RemoveFromTrash(BlueprintItem item)
	{
		_itemsMarkedAsTrash.Remove(item);
	}

	public void RemoveFromTrash(ItemEntity item)
	{
		RemoveFromTrash(item.Blueprint);
	}

	public ItemsCollection GetSharedCollection(BlueprintSharedVendorTable blueprint)
	{
		if (blueprint == null)
		{
			PFLog.Default.Error("BlueprintSharedVendorTable is null");
			return null;
		}
		if (!_tables.TryGetValue(blueprint, out var value))
		{
			value = (_tables[blueprint] = CreateCollectionFromTable(blueprint));
		}
		else
		{
			SyncWithTable(value, blueprint);
		}
		return value;
	}

	private ItemsCollection CreateCollectionFromTable(BlueprintSharedVendorTable table)
	{
		ItemsCollection itemsCollection = new ItemsCollection(Game.Instance.Player)
		{
			ForceStackable = true,
			IsVendorTable = true,
			KeepItemsWithZeroCount = true
		};
		VendorTableSlot[] slots = table.Slots;
		foreach (VendorTableSlot vendorTableSlot in slots)
		{
			if (!(vendorTableSlot?.Item == null))
			{
				ItemEntity itemEntity = vendorTableSlot.Item.Blueprint.CreateEntity();
				itemEntity.IncrementCount(vendorTableSlot.Count - 1);
				itemEntity.SetVendorData(table, vendorTableSlot);
				itemsCollection.Add(itemEntity);
			}
		}
		return itemsCollection;
	}

	private void SyncWithTable(ItemsCollection collection, BlueprintSharedVendorTable table)
	{
		List<VendorTableSlot> value;
		using (CollectionPool<List<VendorTableSlot>, VendorTableSlot>.Get(out value))
		{
			List<ItemEntity> value2;
			using (CollectionPool<List<ItemEntity>, ItemEntity>.Get(out value2))
			{
				foreach (ItemEntity item in collection.Items)
				{
					if (!item.IsSoldByPlayer())
					{
						VendorTableSlot vendorTableSlot = table.FindVendorSlot(item);
						if (vendorTableSlot == null || vendorTableSlot.Item != item.Blueprint || vendorTableSlot.Disable)
						{
							value2.Add(item);
						}
						else if (item.Count > vendorTableSlot.Count)
						{
							item.DecrementCount(item.Count - vendorTableSlot.Count);
						}
					}
				}
				foreach (ItemEntity item2 in value2)
				{
					collection.Remove(item2);
				}
				VendorTableSlot[] slots = table.Slots;
				foreach (VendorTableSlot slot in slots)
				{
					if (!(slot?.Item == null) && !slot.Disable && collection.Items.FirstItem((ItemEntity i) => i.IsFromVendorSlot(table, slot)) == null)
					{
						value.Add(slot);
					}
				}
				foreach (VendorTableSlot item3 in value)
				{
					ItemEntity itemEntity = item3.Item.Blueprint.CreateEntity();
					itemEntity.IncrementCount(item3.Count - 1);
					itemEntity.SetVendorData(table, item3);
					collection.Add(itemEntity);
				}
			}
		}
	}

	public void AddDetectedVendor(MechanicEntity entity, [NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		_detectedVendors.Add(new DetectedVendorData(entity, area, areaPart, chapter));
	}

	public void ClearDetectedVendors()
	{
		_detectedVendors.Clear();
	}

	public void ReplenishAll()
	{
		foreach (var (table, collection) in _tables)
		{
			Replenish(table, collection);
		}
	}

	public void Replenish(BlueprintSharedVendorTable table)
	{
		ItemsCollection valueOrDefault = _tables.GetValueOrDefault(table);
		if (valueOrDefault != null)
		{
			Replenish(table, valueOrDefault);
		}
	}

	private void Replenish(BlueprintSharedVendorTable table, ItemsCollection collection)
	{
		foreach (ItemEntity item in collection.Items)
		{
			if (item.IsSoldByPlayer())
			{
				continue;
			}
			VendorTableSlot vendorSlot = item.GetVendorSlot();
			if (vendorSlot != null && !vendorSlot.Replenishable)
			{
				int num = vendorSlot.Count - item.Count;
				if (num > 0)
				{
					item.IncrementCount(num);
				}
			}
		}
	}

	public void AddDiscount(FactionType faction, int discount)
	{
		if (faction == FactionType.None)
		{
			throw new ArgumentException("Faction cannot be None", "faction");
		}
		if (discount < 0 || discount > 100)
		{
			throw new ArgumentOutOfRangeException("discount", discount, "Discount must be in range [0..100]");
		}
		int num = discount;
		if (_discounts.TryGetValue(faction, out var value))
		{
			num += value;
		}
		_discounts[faction] = num;
		base.EventBus.RaiseEvent(delegate(IGainFactionVendorDiscountHandler l)
		{
			l.HandleGainFactionVendorDiscount(faction, discount);
		});
	}

	public int GetDiscount(FactionType faction)
	{
		return Math.Clamp(_discounts.GetValueOrDefault(faction), 0, 100);
	}

	public void CleanupAllTables()
	{
		foreach (var (table, collection) in _tables)
		{
			CleanupTable(table, collection);
		}
	}

	public void CleanupTable(BlueprintSharedVendorTable table)
	{
		ItemsCollection valueOrDefault = _tables.GetValueOrDefault(table);
		if (valueOrDefault != null)
		{
			CleanupTable(table, valueOrDefault);
		}
	}

	private void CleanupTable(BlueprintSharedVendorTable table, ItemsCollection collection)
	{
		List<ItemEntity> value;
		using (CollectionPool<List<ItemEntity>, ItemEntity>.Get(out value))
		{
			TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
			foreach (ItemEntity item in collection)
			{
				if (item.SellTime.HasValue && !(gameTime - item.SellTime.Value < CleanupStoreItemsFrequency) && item.IsTrash())
				{
					value.Add(item);
				}
			}
			foreach (ItemEntity item2 in value)
			{
				collection.Remove(item2);
			}
		}
	}

	protected override void OnPrePostLoad()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.PrePostLoad();
		}
	}

	protected override void OnPostLoad()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.PostLoad();
		}
	}

	protected override void OnPreSave()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.PreSave();
		}
	}

	protected override void OnSubscribe()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.Subscribe();
		}
	}

	protected override void OnUnsubscribe()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.Unsubscribe();
		}
	}

	protected override void OnDispose()
	{
		foreach (ItemsCollection value in _tables.Values)
		{
			value.Dispose();
		}
	}

	void IChangeChapterHandler.HandleChangeChapter()
	{
		ClearDetectedVendors();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintSharedVendorTable, ItemsCollection> tables = _tables;
		if (tables != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintSharedVendorTable, ItemsCollection> item in tables)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<ItemsCollection>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		HashSet<BlueprintItem> itemsMarkedAsTrash = _itemsMarkedAsTrash;
		if (itemsMarkedAsTrash != null)
		{
			int num = 0;
			foreach (BlueprintItem item2 in itemsMarkedAsTrash)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item2).GetHashCode();
			}
			result.Append(num);
		}
		List<DetectedVendorData> detectedVendors = _detectedVendors;
		if (detectedVendors != null)
		{
			for (int i = 0; i < detectedVendors.Count; i++)
			{
				Hash128 val5 = ClassHasher<DetectedVendorData>.GetHash128(detectedVendors[i]);
				result.Append(ref val5);
			}
		}
		Hash128 val6 = ClassHasher<ItemsCollection>.GetHash128(_itemsForBuy);
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<ItemsCollection>.GetHash128(_itemsForSell);
		result.Append(ref val7);
		Dictionary<FactionType, int> discounts = _discounts;
		if (discounts != null)
		{
			int val8 = 0;
			foreach (KeyValuePair<FactionType, int> item3 in discounts)
			{
				Hash128 hash2 = default(Hash128);
				FactionType obj = item3.Key;
				Hash128 val9 = UnmanagedHasher<FactionType>.GetHash128(ref obj);
				hash2.Append(ref val9);
				int obj2 = item3.Value;
				Hash128 val10 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val10);
				val8 ^= hash2.GetHashCode();
			}
			result.Append(ref val8);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		VendorsManager source = new VendorsManager(default(OwlPackConstructorParameter));
		result = Unsafe.As<VendorsManager, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<VendorsManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "_tables", ref _tables, state);
		formatter.Field(11, "_itemsMarkedAsTrash", ref _itemsMarkedAsTrash, state);
		formatter.Field(12, "_detectedVendors", ref _detectedVendors, state);
		formatter.Field(13, "_itemsForBuy", ref _itemsForBuy, state);
		formatter.Field(14, "_itemsForSell", ref _itemsForSell, state);
		formatter.Field(15, "_discounts", ref _discounts, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VendorsManager>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				_tables = formatter.ReadPackable<Dictionary<BlueprintSharedVendorTable, ItemsCollection>>(state);
				break;
			case 11:
				_itemsMarkedAsTrash = formatter.ReadPackable<HashSet<BlueprintItem>>(state);
				break;
			case 12:
				_detectedVendors = formatter.ReadPackable<List<DetectedVendorData>>(state);
				break;
			case 13:
				_itemsForBuy = formatter.ReadPackable<ItemsCollection>(state);
				break;
			case 14:
				_itemsForSell = formatter.ReadPackable<ItemsCollection>(state);
				break;
			case 15:
				_discounts = formatter.ReadPackable<Dictionary<FactionType, int>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
