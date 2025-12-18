using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
public class SaveMetaAcknowledgeData : IOwlPackable, IOwlPackable<SaveMetaAcknowledgeData>
{
	public struct GuidData
	{
		public Guid Guid;

		public bool AlreadyHave;
	}

	[JsonProperty(PropertyName = "g")]
	[OwlPackInclude]
	public GuidData[] PortraitsGuid;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveMetaAcknowledgeData",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("PortraitsGuid", typeof(GuidData[]))
		}
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveMetaAcknowledgeData source = new SaveMetaAcknowledgeData();
		result = Unsafe.As<SaveMetaAcknowledgeData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SaveMetaAcknowledgeData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "PortraitsGuid", ref PortraitsGuid, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveMetaAcknowledgeData>();
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
				PortraitsGuid = formatter.ReadPackable<GuidData[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
