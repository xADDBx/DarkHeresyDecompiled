using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem.ContextData;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[OwlPackable(OwlPackableMode.Generate)]
public class Rand : IHashable, IOwlPackable, IOwlPackable<Rand>
{
	[JsonProperty]
	[OwlPackInclude]
	public RandState State;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Rand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("State", typeof(RandState))
		}
	};

	public uint Seed
	{
		get
		{
			return State.x;
		}
		set
		{
			State.x = value;
			State.y = State.x * 1812433253 + 1;
			State.z = State.y * 1812433253 + 1;
			State.w = State.z * 1812433253 + 1;
		}
	}

	public Rand()
		: this(0u)
	{
	}

	public Rand(uint seed)
	{
		Seed = seed;
	}

	public uint Get()
	{
		if ((bool)ContextData<DisableStatefulRandomContext>.Current)
		{
			return (uint)Random.Range(0, int.MaxValue);
		}
		uint num = State.x ^ (State.x << 11);
		State.x = State.y;
		State.y = State.z;
		State.z = State.w;
		return State.w = State.w ^ (State.w >> 19) ^ (num ^ (num >> 8));
	}

	public ulong Get64()
	{
		long num = Get();
		ulong num2 = Get();
		return (ulong)(num << 32) | num2;
	}

	public static float GetFloatFromInt(uint value)
	{
		return (float)(value & 0x7FFFFFu) * 1.192093E-07f;
	}

	public float RangedRandom(float inclusiveMin, float inclusiveMax)
	{
		float @float = GetFloat();
		return inclusiveMin * @float + (1f - @float) * inclusiveMax;
	}

	public int RangedRandom(int inclusiveMin, int exclusiveMax)
	{
		if (inclusiveMin < exclusiveMax)
		{
			int num = exclusiveMax - inclusiveMin;
			return (int)(Get() % num) + inclusiveMin;
		}
		if (exclusiveMax < inclusiveMin)
		{
			int num2 = inclusiveMin - exclusiveMax;
			int num3 = (int)(Get() % num2);
			return inclusiveMin - num3;
		}
		return inclusiveMin;
	}

	public float GetFloat()
	{
		return GetFloatFromInt(Get());
	}

	public float GetSignedFloat()
	{
		return GetFloat() * 2f - 1f;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref State);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Rand source = new Rand();
		result = Unsafe.As<Rand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Rand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "State", ref State, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Rand>();
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
				State = formatter.ReadPackable<RandState>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
