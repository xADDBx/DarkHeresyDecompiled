using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine.Experimental.Rendering;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public struct ImageMetaData : IOwlPackable, IOwlPackable<ImageMetaData>
{
	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "l")]
	public int SaveLength;

	[JsonProperty(PropertyName = "w")]
	public int Width;

	[JsonProperty(PropertyName = "h")]
	public int Height;

	[JsonProperty(PropertyName = "f")]
	public GraphicsFormat Format;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ImageMetaData",
		Fields = new FieldInfo[0]
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ImageMetaData source = default(ImageMetaData);
		result = Unsafe.As<ImageMetaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ImageMetaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ImageMetaData>();
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
