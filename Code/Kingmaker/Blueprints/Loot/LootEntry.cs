using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class LootEntry : IOwlPackable, IOwlPackable<LootEntry>
{
	[SerializeField]
	[GameStateInclude]
	private BlueprintItemReference m_Item;

	[SerializeField]
	[JsonProperty]
	[OwlPackInclude]
	public int Diversity;

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	[OwlPackInclude]
	public int Count = 1;

	[NonSerialized]
	public bool Identify;

	[SerializeField]
	private int m_ReputationPointsToUnlock;

	[SerializeField]
	private long? m_ProfitFactorCostOverride;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LootEntry",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Item", typeof(BlueprintItem)),
			new FieldInfo("Diversity", typeof(int)),
			new FieldInfo("Count", typeof(int))
		}
	};

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
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

	public long? ProfitFactorCostOverride
	{
		get
		{
			return m_ProfitFactorCostOverride;
		}
		set
		{
			m_ProfitFactorCostOverride = value;
		}
	}

	public int ReputationPointsToUnlock
	{
		get
		{
			return m_ReputationPointsToUnlock;
		}
		set
		{
			m_ReputationPointsToUnlock = value;
		}
	}

	public float ProfitFactorCost => Item ? (Item.Cost * Count) : 0;

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LootEntry source = new LootEntry();
		result = Unsafe.As<LootEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LootEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintItem value = Item;
		formatter.Field(0, "Item", ref value, state);
		formatter.UnmanagedField(1, "Diversity", ref Diversity, state);
		formatter.UnmanagedField(2, "Count", ref Count, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LootEntry>();
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
				Item = formatter.ReadPackable<BlueprintItem>(state);
				break;
			case 1:
				Diversity = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
