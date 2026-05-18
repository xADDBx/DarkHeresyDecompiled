using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class LeftStickData : IMemoryPackable<LeftStickData>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<LeftStickData>
{
	[Preserve]
	private sealed class LeftStickDataFormatter : MemoryPackFormatter<LeftStickData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref LeftStickData value)
		{
			LeftStickData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref LeftStickData value)
		{
			LeftStickData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LeftStickData value)
		{
			LeftStickData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref LeftStickData value)
		{
			LeftStickData.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "v")]
	[OwlPackInclude]
	public byte version;

	[JsonProperty(PropertyName = "u")]
	[OwlPackInclude]
	public UnitReference unit;

	[JsonProperty(PropertyName = "x")]
	[OwlPackInclude]
	public sbyte moveDirectionX;

	[JsonProperty(PropertyName = "y")]
	[OwlPackInclude]
	public sbyte moveDirectionY;

	[JsonProperty(PropertyName = "s")]
	[OwlPackInclude]
	public UnitReference[] selectedUnits;

	public static readonly TypeInfo OwlPackTypeInfo;

	static LeftStickData()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "LeftStickData",
			OldNames = null,
			Fields = new FieldInfo[5]
			{
				new FieldInfo("version", typeof(byte)),
				new FieldInfo("unit", typeof(UnitReference)),
				new FieldInfo("moveDirectionX", typeof(sbyte)),
				new FieldInfo("moveDirectionY", typeof(sbyte)),
				new FieldInfo("selectedUnits", typeof(UnitReference[]))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LeftStickData>())
		{
			MemoryPackFormatterProvider.Register(new LeftStickDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LeftStickData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LeftStickData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref LeftStickData? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(5, in value.version);
		writer.WritePackable(in value.unit);
		writer.WriteUnmanaged(in value.moveDirectionX, in value.moveDirectionY);
		writer.WritePackableArray(value.selectedUnits);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LeftStickData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		UnitReference value3;
		sbyte value4;
		sbyte value5;
		UnitReference[] value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.version;
				value3 = value.unit;
				value4 = value.moveDirectionX;
				value5 = value.moveDirectionY;
				value6 = value.selectedUnits;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<sbyte>(out value4);
				reader.ReadUnmanaged<sbyte>(out value5);
				reader.ReadPackableArray(ref value6);
				goto IL_0131;
			}
			reader.ReadUnmanaged<byte>(out value2);
			value3 = reader.ReadPackable<UnitReference>();
			reader.ReadUnmanaged<sbyte, sbyte>(out value4, out value5);
			value6 = reader.ReadPackableArray<UnitReference>();
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LeftStickData), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = default(UnitReference);
				value4 = 0;
				value5 = 0;
				value6 = null;
			}
			else
			{
				value2 = value.version;
				value3 = value.unit;
				value4 = value.moveDirectionX;
				value5 = value.moveDirectionY;
				value6 = value.selectedUnits;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<sbyte>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<sbyte>(out value5);
							if (memberCount != 4)
							{
								reader.ReadPackableArray(ref value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0131;
			}
		}
		value = new LeftStickData
		{
			version = value2,
			unit = value3,
			moveDirectionX = value4,
			moveDirectionY = value5,
			selectedUnits = value6
		};
		return;
		IL_0131:
		value.version = value2;
		value.unit = value3;
		value.moveDirectionX = value4;
		value.moveDirectionY = value5;
		value.selectedUnits = value6;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref LeftStickData? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("version");
		writer.WriteUnmanaged(value.version);
		writer.WriteProperty("unit");
		writer.WritePackable(value.unit);
		writer.WriteProperty("moveDirectionX");
		writer.WriteUnmanaged(value.moveDirectionX);
		writer.WriteProperty("moveDirectionY");
		writer.WriteUnmanaged(value.moveDirectionY);
		writer.WriteProperty("selectedUnits");
		writer.WritePackableArray(value.selectedUnits);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref LeftStickData? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		byte v;
		UnitReference val;
		sbyte v2;
		sbyte v3;
		UnitReference[] value2;
		if (value == null)
		{
			v = 0;
			val = default(UnitReference);
			v2 = 0;
			v3 = 0;
			value2 = null;
		}
		else
		{
			v = value.version;
			val = value.unit;
			v2 = value.moveDirectionX;
			v3 = value.moveDirectionY;
			value2 = value.selectedUnits;
		}
		bool[] array = new bool[5];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "version":
					reader.ReadUnmanaged<byte>(out v);
					array[0] = true;
					break;
				case "unit":
					val = reader.ReadPackable<UnitReference>();
					array[1] = true;
					break;
				case "moveDirectionX":
					reader.ReadUnmanaged<sbyte>(out v2);
					array[2] = true;
					break;
				case "moveDirectionY":
					reader.ReadUnmanaged<sbyte>(out v3);
					array[3] = true;
					break;
				case "selectedUnits":
					value2 = reader.ReadPackableArray<UnitReference>();
					array[4] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "version":
					reader.ReadUnmanaged<byte>(out v);
					break;
				case "unit":
					reader.ReadPackable(ref val);
					break;
				case "moveDirectionX":
					reader.ReadUnmanaged<sbyte>(out v2);
					break;
				case "moveDirectionY":
					reader.ReadUnmanaged<sbyte>(out v3);
					break;
				case "selectedUnits":
					reader.ReadPackableArray(ref value2);
					break;
				}
			}
		}
		if (value != null)
		{
			value.version = v;
			value.unit = val;
			value.moveDirectionX = v2;
			value.moveDirectionY = v3;
			value.selectedUnits = value2;
		}
		else
		{
			value = new LeftStickData
			{
				version = v,
				unit = val,
				moveDirectionX = v2,
				moveDirectionY = v3,
				selectedUnits = value2
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
		LeftStickData source = new LeftStickData();
		result = Unsafe.As<LeftStickData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LeftStickData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "version", ref version, state);
		formatter.Field(1, "unit", ref unit, state);
		formatter.UnmanagedField(2, "moveDirectionX", ref moveDirectionX, state);
		formatter.UnmanagedField(3, "moveDirectionY", ref moveDirectionY, state);
		formatter.Field(4, "selectedUnits", ref selectedUnits, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LeftStickData>();
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
				version = formatter.ReadUnmanaged<byte>(state);
				break;
			case 1:
				unit = formatter.ReadPackable<UnitReference>(state);
				break;
			case 2:
				moveDirectionX = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 3:
				moveDirectionY = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 4:
				selectedUnits = formatter.ReadPackable<UnitReference[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
