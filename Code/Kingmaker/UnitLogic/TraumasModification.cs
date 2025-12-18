using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class TraumasModification : IHashable, IOwlPackable, IOwlPackable<TraumasModification>
{
	[JsonProperty]
	[OwlPackInclude]
	public int OldWoundDelayRoundsModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int WoundStacksForTraumaModifier;

	[JsonProperty]
	[OwlPackInclude]
	public int WoundDamagePerTurnThresholdHPFractionModifier;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TraumasModification",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("OldWoundDelayRoundsModifier", typeof(int)),
			new FieldInfo("WoundStacksForTraumaModifier", typeof(int)),
			new FieldInfo("WoundDamagePerTurnThresholdHPFractionModifier", typeof(int))
		}
	};

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref OldWoundDelayRoundsModifier);
		result.Append(ref WoundStacksForTraumaModifier);
		result.Append(ref WoundDamagePerTurnThresholdHPFractionModifier);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TraumasModification source = new TraumasModification();
		result = Unsafe.As<TraumasModification, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TraumasModification>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "OldWoundDelayRoundsModifier", ref OldWoundDelayRoundsModifier, state);
		formatter.UnmanagedField(1, "WoundStacksForTraumaModifier", ref WoundStacksForTraumaModifier, state);
		formatter.UnmanagedField(2, "WoundDamagePerTurnThresholdHPFractionModifier", ref WoundDamagePerTurnThresholdHPFractionModifier, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TraumasModification>();
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
				OldWoundDelayRoundsModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				WoundStacksForTraumaModifier = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				WoundDamagePerTurnThresholdHPFractionModifier = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
