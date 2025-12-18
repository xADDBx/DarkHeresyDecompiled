using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct RandState : IHashable, IOwlPackable, IOwlPackable<RandState>
{
	[JsonProperty]
	[OwlPackInclude]
	public uint x;

	[JsonProperty]
	[OwlPackInclude]
	public uint y;

	[JsonProperty]
	[OwlPackInclude]
	public uint z;

	[JsonProperty]
	[OwlPackInclude]
	public uint w;

	public static readonly RandState Default = new Rand(0u).State;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RandState",
		Fields = new FieldInfo[4]
		{
			new FieldInfo("x", typeof(uint)),
			new FieldInfo("y", typeof(uint)),
			new FieldInfo("z", typeof(uint)),
			new FieldInfo("w", typeof(uint))
		}
	};

	public bool IsReady
	{
		get
		{
			if (x == 0 && y == 0 && z == 0)
			{
				return w != 0;
			}
			return true;
		}
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref x);
		result.Append(ref y);
		result.Append(ref z);
		result.Append(ref w);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RandState source = default(RandState);
		result = Unsafe.As<RandState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RandState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "x", ref x, state);
		formatter.UnmanagedField(1, "y", ref y, state);
		formatter.UnmanagedField(2, "z", ref z, state);
		formatter.UnmanagedField(3, "w", ref w, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RandState>();
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
				x = formatter.ReadUnmanaged<uint>(state);
				break;
			case 1:
				y = formatter.ReadUnmanaged<uint>(state);
				break;
			case 2:
				z = formatter.ReadUnmanaged<uint>(state);
				break;
			case 3:
				w = formatter.ReadUnmanaged<uint>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
