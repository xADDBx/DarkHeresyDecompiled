using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PortraitEntry : IMemoryPackable<PortraitEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<PortraitEntry>
{
	[Preserve]
	private sealed class PortraitEntryFormatter : MemoryPackFormatter<PortraitEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PortraitEntry value)
		{
			PortraitEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PortraitEntry value)
		{
			PortraitEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PortraitEntry value)
		{
			PortraitEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PortraitEntry value)
		{
			PortraitEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly BlueprintPortraitReference Portrait;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public PortraitEntry([NotNull] BlueprintPortraitReference portrait)
	{
		Portrait = portrait;
	}

	public PortraitEntry(OwlPackConstructorParameter _)
	{
	}

	static PortraitEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PortraitEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitEntry>())
		{
			MemoryPackFormatterProvider.Register(new PortraitEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PortraitEntry>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PortraitEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.Portrait);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PortraitEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPortraitReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintPortraitReference>();
			}
			else
			{
				value2 = value.Portrait;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PortraitEntry), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Portrait : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PortraitEntry(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PortraitEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Portrait");
		writer.WritePackable(value.Portrait);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PortraitEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintPortraitReference val = ((value != null) ? value.Portrait : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Portrait")
				{
					val = reader.ReadPackable<BlueprintPortraitReference>();
					array[0] = true;
				}
			}
			else if (text == "Portrait")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new PortraitEntry(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PortraitEntry source = new PortraitEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<PortraitEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PortraitEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PortraitEntry>();
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
