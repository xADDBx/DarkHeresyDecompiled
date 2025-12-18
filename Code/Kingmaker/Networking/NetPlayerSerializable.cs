using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public readonly struct NetPlayerSerializable : IOwlPackable, IOwlPackable<NetPlayerSerializable>
{
	private const short EmptyIndex = -1;

	[JsonProperty(PropertyName = "i")]
	[OwlPackInclude]
	public readonly short Index;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NetPlayerSerializable",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Index", typeof(short))
		}
	};

	private NetPlayerSerializable(short index)
	{
		Index = index;
	}

	public NetPlayerSerializable(NetPlayer player)
		: this((short)(player.IsEmpty ? (-1) : ((short)player.Index)))
	{
	}

	public static explicit operator NetPlayer(NetPlayerSerializable value)
	{
		if (value.Index == -1)
		{
			return NetPlayer.Empty;
		}
		if (value.Index == NetPlayer.Offline.Index)
		{
			return NetPlayer.Offline;
		}
		bool isLocal = value.Index == NetworkingManager.LocalNetPlayer.Index;
		return new NetPlayer(value.Index, isLocal);
	}

	public static explicit operator NetPlayerSerializable(NetPlayer value)
	{
		return new NetPlayerSerializable(value);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NetPlayerSerializable source = default(NetPlayerSerializable);
		result = Unsafe.As<NetPlayerSerializable, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NetPlayerSerializable>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		short value = Index;
		formatter.UnmanagedField(0, "Index", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NetPlayerSerializable>();
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
				Unsafe.AsRef(in Index) = formatter.ReadUnmanaged<short>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
