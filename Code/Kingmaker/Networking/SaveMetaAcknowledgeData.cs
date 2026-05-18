using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SaveMetaAcknowledgeData : IMemoryPackable<SaveMetaAcknowledgeData>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<SaveMetaAcknowledgeData>
{
	public struct GuidData
	{
		public Guid Guid;

		public bool AlreadyHave;
	}

	[Preserve]
	private sealed class SaveMetaAcknowledgeDataFormatter : MemoryPackFormatter<SaveMetaAcknowledgeData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "g")]
	[OwlPackInclude]
	public GuidData[] PortraitsGuid;

	public static readonly TypeInfo OwlPackTypeInfo;

	static SaveMetaAcknowledgeData()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SaveMetaAcknowledgeData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("PortraitsGuid", typeof(GuidData[]))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaAcknowledgeData>())
		{
			MemoryPackFormatterProvider.Register(new SaveMetaAcknowledgeDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaAcknowledgeData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveMetaAcknowledgeData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GuidData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GuidData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveMetaAcknowledgeData? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteUnmanagedArray(value.PortraitsGuid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveMetaAcknowledgeData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		GuidData[] value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.PortraitsGuid;
				reader.ReadUnmanagedArray(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadUnmanagedArray<GuidData>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveMetaAcknowledgeData), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.PortraitsGuid : null);
			if (memberCount != 0)
			{
				reader.ReadUnmanagedArray(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new SaveMetaAcknowledgeData
		{
			PortraitsGuid = value2
		};
		return;
		IL_006a:
		value.PortraitsGuid = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveMetaAcknowledgeData? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("PortraitsGuid");
		writer.WriteArray(value.PortraitsGuid);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveMetaAcknowledgeData? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		GuidData[] value2 = ((value != null) ? value.PortraitsGuid : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "PortraitsGuid")
				{
					value2 = reader.ReadArray<GuidData>();
					array[0] = true;
				}
			}
			else if (text == "PortraitsGuid")
			{
				reader.ReadArray(ref value2);
			}
		}
		if (value != null)
		{
			value.PortraitsGuid = value2;
		}
		else
		{
			value = new SaveMetaAcknowledgeData
			{
				PortraitsGuid = value2
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

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
