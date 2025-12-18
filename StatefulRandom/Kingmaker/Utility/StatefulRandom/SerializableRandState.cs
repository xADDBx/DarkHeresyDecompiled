using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Utility.StatefulRandom;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public readonly struct SerializableRandState : IOwlPackable, IOwlPackable<SerializableRandState>
{
	[JsonProperty]
	[OwlPackInclude]
	public readonly string Name;

	[JsonProperty]
	[OwlPackInclude]
	public readonly uint x;

	[JsonProperty]
	[OwlPackInclude]
	public readonly uint y;

	[JsonProperty]
	[OwlPackInclude]
	public readonly uint z;

	[JsonProperty]
	[OwlPackInclude]
	public readonly uint w;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SerializableRandState",
		Fields = new FieldInfo[5]
		{
			new FieldInfo("Name", typeof(string)),
			new FieldInfo("x", typeof(uint)),
			new FieldInfo("y", typeof(uint)),
			new FieldInfo("z", typeof(uint)),
			new FieldInfo("w", typeof(uint))
		}
	};

	public RandState Value
	{
		get
		{
			RandState result = default(RandState);
			result.x = x;
			result.y = y;
			result.z = z;
			result.w = w;
			return result;
		}
	}

	private SerializableRandState(string name, uint x, uint y, uint z, uint w)
	{
		Name = name;
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public SerializableRandState(string name, RandState value)
	{
		Name = name;
		x = value.x;
		y = value.y;
		z = value.z;
		w = value.w;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SerializableRandState source = default(SerializableRandState);
		result = Unsafe.As<SerializableRandState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SerializableRandState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Name;
		formatter.StringField(0, "Name", ref value, state);
		uint value2 = x;
		formatter.UnmanagedField(1, "x", ref value2, state);
		uint value3 = y;
		formatter.UnmanagedField(2, "y", ref value3, state);
		uint value4 = z;
		formatter.UnmanagedField(3, "z", ref value4, state);
		uint value5 = w;
		formatter.UnmanagedField(4, "w", ref value5, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SerializableRandState>();
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
				Unsafe.AsRef(in Name) = formatter.ReadString(state);
				break;
			case 1:
				Unsafe.AsRef(in x) = formatter.ReadUnmanaged<uint>(state);
				break;
			case 2:
				Unsafe.AsRef(in y) = formatter.ReadUnmanaged<uint>(state);
				break;
			case 3:
				Unsafe.AsRef(in z) = formatter.ReadUnmanaged<uint>(state);
				break;
			case 4:
				Unsafe.AsRef(in w) = formatter.ReadUnmanaged<uint>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
