using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Reputation;

[OwlPackable(OwlPackableMode.Generate)]
public readonly struct ReputationDescription : IHashable, IOwlPackable, IOwlPackable<ReputationDescription>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly int Respect;

	[JsonProperty]
	[OwlPackInclude]
	public readonly int Fear;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ReputationDescription",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Respect", typeof(int)),
			new FieldInfo("Fear", typeof(int))
		}
	};

	public ReputationDescription(int fear, int respect)
	{
		Respect = Math.Clamp(respect, 0, 100);
		Fear = Math.Clamp(fear, 0, 100);
	}

	public ReputationDescription AddRespect(int value)
	{
		return new ReputationDescription(Fear, Respect + value);
	}

	public ReputationDescription AddFear(int value)
	{
		return new ReputationDescription(Fear + value, Respect);
	}

	public void Deconstruct(out int fear, out int respect)
	{
		respect = Respect;
		fear = Fear;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		int val = Respect;
		result.Append(ref val);
		int val2 = Fear;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ReputationDescription source = default(ReputationDescription);
		result = Unsafe.As<ReputationDescription, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ReputationDescription>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = Respect;
		formatter.UnmanagedField(0, "Respect", ref value, state);
		int value2 = Fear;
		formatter.UnmanagedField(1, "Fear", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ReputationDescription>();
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
				Unsafe.AsRef(in Respect) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in Fear) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
