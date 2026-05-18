using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Settings;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SaveMetaData : IMemoryPackable<SaveMetaData>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<SaveMetaData>
{
	[Preserve]
	private sealed class SaveMetaDataFormatter : MemoryPackFormatter<SaveMetaData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveMetaData value)
		{
			SaveMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveMetaData value)
		{
			SaveMetaData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveMetaData value)
		{
			SaveMetaData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveMetaData value)
		{
			SaveMetaData.DeserializeJson(ref reader, ref value);
		}
	}

	public static int MaxPacketSize;

	[JsonProperty(PropertyName = "l")]
	[OwlPackInclude]
	public int length;

	[JsonProperty(PropertyName = "n")]
	[OwlPackInclude]
	public string saveName;

	[JsonProperty(PropertyName = "i")]
	[OwlPackInclude]
	public string saveId;

	[JsonProperty(PropertyName = "r")]
	[OwlPackInclude]
	public uint randomNoise;

	[JsonProperty(PropertyName = "a")]
	[OwlPackInclude]
	public PhotonActorNumber[] actorNumbersAtStart;

	[JsonProperty(PropertyName = "d")]
	[OwlPackInclude]
	public string[] dlcs;

	[JsonProperty(PropertyName = "s")]
	[OwlPackInclude]
	public BaseSettingNetData[] settings;

	[JsonProperty(PropertyName = "p")]
	[OwlPackInclude]
	public PortraitSaveMetaData[] portraitsSaveMeta;

	public static readonly TypeInfo OwlPackTypeInfo;

	static SaveMetaData()
	{
		MaxPacketSize = 49152;
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SaveMetaData",
			OldNames = null,
			Fields = new FieldInfo[8]
			{
				new FieldInfo("length", typeof(int)),
				new FieldInfo("saveName", typeof(string)),
				new FieldInfo("saveId", typeof(string)),
				new FieldInfo("randomNoise", typeof(uint)),
				new FieldInfo("actorNumbersAtStart", typeof(PhotonActorNumber[])),
				new FieldInfo("dlcs", typeof(string[])),
				new FieldInfo("settings", typeof(BaseSettingNetData[])),
				new FieldInfo("portraitsSaveMeta", typeof(PortraitSaveMetaData[]))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaData>())
		{
			MemoryPackFormatterProvider.Register(new SaveMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveMetaData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PhotonActorNumber[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PhotonActorNumber>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<string>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BaseSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BaseSettingNetData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitSaveMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PortraitSaveMetaData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SaveMetaData? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(8, in value.length);
		writer.WriteString(value.saveName);
		writer.WriteString(value.saveId);
		writer.WriteUnmanaged(in value.randomNoise);
		writer.WriteUnmanagedArray(value.actorNumbersAtStart);
		writer.WriteArray(value.dlcs);
		writer.WriteArray(value.settings);
		writer.WriteArray(value.portraitsSaveMeta);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveMetaData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		uint value3;
		PhotonActorNumber[] value4;
		string[] value5;
		BaseSettingNetData[] value6;
		PortraitSaveMetaData[] value7;
		string text;
		string text2;
		if (memberCount == 8)
		{
			if (value != null)
			{
				value2 = value.length;
				text = value.saveName;
				text2 = value.saveId;
				value3 = value.randomNoise;
				value4 = value.actorNumbersAtStart;
				value5 = value.dlcs;
				value6 = value.settings;
				value7 = value.portraitsSaveMeta;
				reader.ReadUnmanaged<int>(out value2);
				text = reader.ReadString();
				text2 = reader.ReadString();
				reader.ReadUnmanaged<uint>(out value3);
				reader.ReadUnmanagedArray(ref value4);
				reader.ReadArray(ref value5);
				reader.ReadArray(ref value6);
				reader.ReadArray(ref value7);
				goto IL_01bf;
			}
			reader.ReadUnmanaged<int>(out value2);
			text = reader.ReadString();
			text2 = reader.ReadString();
			reader.ReadUnmanaged<uint>(out value3);
			value4 = reader.ReadUnmanagedArray<PhotonActorNumber>();
			value5 = reader.ReadArray<string>();
			value6 = reader.ReadArray<BaseSettingNetData>();
			value7 = reader.ReadArray<PortraitSaveMetaData>();
		}
		else
		{
			if (memberCount > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveMetaData), 8, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				text = null;
				text2 = null;
				value3 = 0u;
				value4 = null;
				value5 = null;
				value6 = null;
				value7 = null;
			}
			else
			{
				value2 = value.length;
				text = value.saveName;
				text2 = value.saveId;
				value3 = value.randomNoise;
				value4 = value.actorNumbersAtStart;
				value5 = value.dlcs;
				value6 = value.settings;
				value7 = value.portraitsSaveMeta;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					text = reader.ReadString();
					if (memberCount != 2)
					{
						text2 = reader.ReadString();
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<uint>(out value3);
							if (memberCount != 4)
							{
								reader.ReadUnmanagedArray(ref value4);
								if (memberCount != 5)
								{
									reader.ReadArray(ref value5);
									if (memberCount != 6)
									{
										reader.ReadArray(ref value6);
										if (memberCount != 7)
										{
											reader.ReadArray(ref value7);
											_ = 8;
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_01bf;
			}
		}
		value = new SaveMetaData
		{
			length = value2,
			saveName = text,
			saveId = text2,
			randomNoise = value3,
			actorNumbersAtStart = value4,
			dlcs = value5,
			settings = value6,
			portraitsSaveMeta = value7
		};
		return;
		IL_01bf:
		value.length = value2;
		value.saveName = text;
		value.saveId = text2;
		value.randomNoise = value3;
		value.actorNumbersAtStart = value4;
		value.dlcs = value5;
		value.settings = value6;
		value.portraitsSaveMeta = value7;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SaveMetaData? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("length");
		writer.WriteUnmanaged(value.length);
		writer.WriteProperty("saveName");
		writer.WriteString(value.saveName);
		writer.WriteProperty("saveId");
		writer.WriteString(value.saveId);
		writer.WriteProperty("randomNoise");
		writer.WriteUnmanaged(value.randomNoise);
		writer.WriteProperty("actorNumbersAtStart");
		writer.WriteArray(value.actorNumbersAtStart);
		writer.WriteProperty("dlcs");
		writer.WriteArray(value.dlcs);
		writer.WriteProperty("settings");
		writer.WriteArray(value.settings);
		writer.WriteProperty("portraitsSaveMeta");
		writer.WriteArray(value.portraitsSaveMeta);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SaveMetaData? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		string text;
		string text2;
		uint v2;
		PhotonActorNumber[] value2;
		string[] value3;
		BaseSettingNetData[] value4;
		PortraitSaveMetaData[] value5;
		if (value == null)
		{
			v = 0;
			text = null;
			text2 = null;
			v2 = 0u;
			value2 = null;
			value3 = null;
			value4 = null;
			value5 = null;
		}
		else
		{
			v = value.length;
			text = value.saveName;
			text2 = value.saveId;
			v2 = value.randomNoise;
			value2 = value.actorNumbersAtStart;
			value3 = value.dlcs;
			value4 = value.settings;
			value5 = value.portraitsSaveMeta;
		}
		bool[] array = new bool[8];
		string text3 = null;
		while ((text3 = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text3)
				{
				case "length":
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
					break;
				case "saveName":
					text = reader.ReadString();
					array[1] = true;
					break;
				case "saveId":
					text2 = reader.ReadString();
					array[2] = true;
					break;
				case "randomNoise":
					reader.ReadUnmanaged<uint>(out v2);
					array[3] = true;
					break;
				case "actorNumbersAtStart":
					value2 = reader.ReadArray<PhotonActorNumber>();
					array[4] = true;
					break;
				case "dlcs":
					value3 = reader.ReadArray<string>();
					array[5] = true;
					break;
				case "settings":
					value4 = reader.ReadArray<BaseSettingNetData>();
					array[6] = true;
					break;
				case "portraitsSaveMeta":
					value5 = reader.ReadArray<PortraitSaveMetaData>();
					array[7] = true;
					break;
				}
			}
			else
			{
				switch (text3)
				{
				case "length":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "saveName":
					text = reader.ReadString();
					break;
				case "saveId":
					text2 = reader.ReadString();
					break;
				case "randomNoise":
					reader.ReadUnmanaged<uint>(out v2);
					break;
				case "actorNumbersAtStart":
					reader.ReadArray(ref value2);
					break;
				case "dlcs":
					reader.ReadArray(ref value3);
					break;
				case "settings":
					reader.ReadArray(ref value4);
					break;
				case "portraitsSaveMeta":
					reader.ReadArray(ref value5);
					break;
				}
			}
		}
		if (value != null)
		{
			value.length = v;
			value.saveName = text;
			value.saveId = text2;
			value.randomNoise = v2;
			value.actorNumbersAtStart = value2;
			value.dlcs = value3;
			value.settings = value4;
			value.portraitsSaveMeta = value5;
		}
		else
		{
			value = new SaveMetaData
			{
				length = v,
				saveName = text,
				saveId = text2,
				randomNoise = v2,
				actorNumbersAtStart = value2,
				dlcs = value3,
				settings = value4,
				portraitsSaveMeta = value5
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
		SaveMetaData source = new SaveMetaData();
		result = Unsafe.As<SaveMetaData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SaveMetaData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "length", ref length, state);
		formatter.StringField(1, "saveName", ref saveName, state);
		formatter.StringField(2, "saveId", ref saveId, state);
		formatter.UnmanagedField(3, "randomNoise", ref randomNoise, state);
		formatter.Field(4, "actorNumbersAtStart", ref actorNumbersAtStart, state);
		formatter.Field(5, "dlcs", ref dlcs, state);
		formatter.Field(6, "settings", ref settings, state);
		formatter.Field(7, "portraitsSaveMeta", ref portraitsSaveMeta, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveMetaData>();
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
				length = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				saveName = formatter.ReadString(state);
				break;
			case 2:
				saveId = formatter.ReadString(state);
				break;
			case 3:
				randomNoise = formatter.ReadUnmanaged<uint>(state);
				break;
			case 4:
				actorNumbersAtStart = formatter.ReadPackable<PhotonActorNumber[]>(state);
				break;
			case 5:
				dlcs = formatter.ReadPackable<string[]>(state);
				break;
			case 6:
				settings = formatter.ReadPackable<BaseSettingNetData[]>(state);
				break;
			case 7:
				portraitsSaveMeta = formatter.ReadPackable<PortraitSaveMetaData[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
