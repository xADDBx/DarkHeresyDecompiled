using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public struct CustomPortraitMetaData : IOwlPackable, IOwlPackable<CustomPortraitMetaData>
{
	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "s")]
	public int LengthSmallPortrait;

	[JsonProperty(PropertyName = "h")]
	public int LengthHalfPortrait;

	[JsonProperty(PropertyName = "f")]
	public int LengthFullPortrait;

	[JsonProperty(PropertyName = "i")]
	public string CustomPortraitId;

	[JsonProperty(PropertyName = "g")]
	public Guid PortraitGuid;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CustomPortraitMetaData",
		Fields = new FieldInfo[0]
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CustomPortraitMetaData source = default(CustomPortraitMetaData);
		result = Unsafe.As<CustomPortraitMetaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CustomPortraitMetaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CustomPortraitMetaData>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
