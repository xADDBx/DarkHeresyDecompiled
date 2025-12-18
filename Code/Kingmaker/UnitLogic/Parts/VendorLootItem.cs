using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Parts;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class VendorLootItem : IEquatable<VendorLootItem>, IComparable<VendorLootItem>, IOwlPackable, IOwlPackable<VendorLootItem>
{
	[JsonProperty]
	[GameStateInclude]
	[OwlPackInclude]
	private BlueprintItemReference m_Item;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "VendorLootItem",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("m_Item", typeof(BlueprintItemReference)),
			new FieldInfo("Diversity", typeof(int)),
			new FieldInfo("Count", typeof(int)),
			new FieldInfo("ReputationToUnlock", typeof(int)),
			new FieldInfo("ProfitFactorCosts", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int Diversity { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Count { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int ReputationToUnlock { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int ProfitFactorCosts { get; private set; }

	public BlueprintItem Item
	{
		get
		{
			return m_Item?.Get();
		}
		set
		{
			m_Item = value.ToReference<BlueprintItemReference>();
		}
	}

	[JsonConstructor]
	private VendorLootItem()
	{
	}

	public VendorLootItem(LootItemsPackFixed pack)
		: this(pack.Item.Item, pack.Item.Diversity, pack.Count, pack.ReputationPointsToUnlock, pack.Item.ProfitFactorCostOverride.GetValueOrDefault())
	{
	}

	public VendorLootItem(BlueprintItem item, int diversity, int count, int reputationToUnlock, int profitFactorCosts)
	{
		Item = item;
		Diversity = diversity;
		Count = count;
		ReputationToUnlock = reputationToUnlock;
		ProfitFactorCosts = profitFactorCosts;
	}

	public void UpdateCount(int newCount)
	{
		Count += newCount;
	}

	public bool Equals(VendorLootItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Item, other.Item) && Count == other.Count && ReputationToUnlock == other.ReputationToUnlock && ProfitFactorCosts == other.ProfitFactorCosts)
		{
			return Diversity == other.Diversity;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((VendorLootItem)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Item, ReputationToUnlock, ProfitFactorCosts, Diversity);
	}

	public int CompareTo(VendorLootItem other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = Count.CompareTo(other.Count);
		if (num != 0)
		{
			return num;
		}
		int num2 = ReputationToUnlock.CompareTo(other.ReputationToUnlock);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = Diversity.CompareTo(other.Diversity);
		if (num3 != 0)
		{
			return num3;
		}
		return ProfitFactorCosts.CompareTo(other.ProfitFactorCosts);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		VendorLootItem source = new VendorLootItem();
		result = Unsafe.As<VendorLootItem, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<VendorLootItem>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		int value = Diversity;
		formatter.UnmanagedField(1, "Diversity", ref value, state);
		int value2 = Count;
		formatter.UnmanagedField(2, "Count", ref value2, state);
		int value3 = ReputationToUnlock;
		formatter.UnmanagedField(3, "ReputationToUnlock", ref value3, state);
		int value4 = ProfitFactorCosts;
		formatter.UnmanagedField(4, "ProfitFactorCosts", ref value4, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VendorLootItem>();
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
				m_Item = formatter.ReadPackable<BlueprintItemReference>(state);
				break;
			case 1:
				Diversity = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Count = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				ReputationToUnlock = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				ProfitFactorCosts = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
