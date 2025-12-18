using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking.Player;

[OwlPackable(OwlPackableMode.Generate)]
public readonly struct PlayerAvatar : IOwlPackable, IOwlPackable<PlayerAvatar>
{
	public static readonly PlayerAvatar Invalid = default(PlayerAvatar);

	[JsonProperty]
	[OwlPackInclude]
	public readonly ushort Width;

	[JsonProperty]
	[OwlPackInclude]
	public readonly byte[] Data;

	[JsonProperty]
	[OwlPackInclude]
	public readonly bool IsCompressed;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PlayerAvatar",
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Width", typeof(ushort)),
			new FieldInfo("Data", typeof(byte[])),
			new FieldInfo("IsCompressed", typeof(bool))
		}
	};

	public bool IsValid
	{
		get
		{
			if (0 < Width && Data != null)
			{
				return Width < Data.Length;
			}
			return false;
		}
	}

	[JsonConstructor]
	private PlayerAvatar(ushort width, byte[] data, bool isCompressed)
	{
		Width = width;
		Data = data;
		IsCompressed = isCompressed;
	}

	public PlayerAvatar(int width, byte[] data, bool isCompressed = false)
		: this((ushort)width, data, isCompressed)
	{
		if (65535 < width)
		{
			throw new OverflowException($"width={width}/{ushort.MaxValue}");
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PlayerAvatar source = default(PlayerAvatar);
		result = Unsafe.As<PlayerAvatar, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PlayerAvatar>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		ushort value = Width;
		formatter.UnmanagedField(0, "Width", ref value, state);
		byte[] value2 = Data;
		formatter.Field(1, "Data", ref value2, state);
		bool value3 = IsCompressed;
		formatter.UnmanagedField(2, "IsCompressed", ref value3, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PlayerAvatar>();
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
				Unsafe.AsRef(in Width) = formatter.ReadUnmanaged<ushort>(state);
				break;
			case 1:
				Unsafe.AsRef(in Data) = formatter.ReadPackable<byte[]>(state);
				break;
			case 2:
				Unsafe.AsRef(in IsCompressed) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
