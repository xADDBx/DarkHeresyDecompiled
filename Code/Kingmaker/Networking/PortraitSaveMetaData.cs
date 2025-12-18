using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public class PortraitSaveMetaData : IOwlPackable, IOwlPackable<PortraitSaveMetaData>
{
	[JsonProperty(PropertyName = "g")]
	[OwlPackInclude]
	public Guid guid;

	[JsonProperty(PropertyName = "i")]
	[OwlPackInclude]
	public string originId;

	[JsonProperty(PropertyName = "l")]
	[OwlPackInclude]
	public int[] imagesFileLength;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PortraitSaveMetaData",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("guid", typeof(Guid)),
			new FieldInfo("originId", typeof(string)),
			new FieldInfo("imagesFileLength", typeof(int[]))
		}
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PortraitSaveMetaData source = new PortraitSaveMetaData();
		result = Unsafe.As<PortraitSaveMetaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PortraitSaveMetaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "guid", ref guid, state);
		formatter.StringField(1, "originId", ref originId, state);
		formatter.Field(2, "imagesFileLength", ref imagesFileLength, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PortraitSaveMetaData>();
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
				guid = formatter.ReadPackable<Guid>(state);
				break;
			case 1:
				originId = formatter.ReadString(state);
				break;
			case 2:
				imagesFileLength = formatter.ReadPackable<int[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
