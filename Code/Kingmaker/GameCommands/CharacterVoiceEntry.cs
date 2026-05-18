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
public sealed class CharacterVoiceEntry : IMemoryPackable<CharacterVoiceEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<CharacterVoiceEntry>
{
	[Preserve]
	private sealed class CharacterVoiceEntryFormatter : MemoryPackFormatter<CharacterVoiceEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterVoiceEntry value)
		{
			CharacterVoiceEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterVoiceEntry value)
		{
			CharacterVoiceEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharacterVoiceEntry value)
		{
			CharacterVoiceEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterVoiceEntry value)
		{
			CharacterVoiceEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly BlueprintUnitAsksListReference Asks;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public CharacterVoiceEntry([NotNull] BlueprintUnitAsksListReference asks)
	{
		Asks = asks;
	}

	public CharacterVoiceEntry(OwlPackConstructorParameter _)
	{
	}

	static CharacterVoiceEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharacterVoiceEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterVoiceEntry>())
		{
			MemoryPackFormatterProvider.Register(new CharacterVoiceEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterVoiceEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharacterVoiceEntry>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterVoiceEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.Asks);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharacterVoiceEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintUnitAsksListReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintUnitAsksListReference>();
			}
			else
			{
				value2 = value.Asks;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharacterVoiceEntry), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Asks : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharacterVoiceEntry(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterVoiceEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Asks");
		writer.WritePackable(value.Asks);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterVoiceEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintUnitAsksListReference val = ((value != null) ? value.Asks : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Asks")
				{
					val = reader.ReadPackable<BlueprintUnitAsksListReference>();
					array[0] = true;
				}
			}
			else if (text == "Asks")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharacterVoiceEntry(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharacterVoiceEntry source = new CharacterVoiceEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharacterVoiceEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharacterVoiceEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharacterVoiceEntry>();
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
