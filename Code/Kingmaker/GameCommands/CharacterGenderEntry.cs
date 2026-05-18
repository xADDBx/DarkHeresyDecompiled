using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharacterGenderEntry : IMemoryPackable<CharacterGenderEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<CharacterGenderEntry>
{
	[Preserve]
	private sealed class CharacterGenderEntryFormatter : MemoryPackFormatter<CharacterGenderEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterGenderEntry value)
		{
			CharacterGenderEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterGenderEntry value)
		{
			CharacterGenderEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharacterGenderEntry value)
		{
			CharacterGenderEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterGenderEntry value)
		{
			CharacterGenderEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly Gender Gender;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public CharacterGenderEntry(Gender gender)
	{
		Gender = gender;
	}

	public CharacterGenderEntry(OwlPackConstructorParameter _)
	{
	}

	static CharacterGenderEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharacterGenderEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterGenderEntry>())
		{
			MemoryPackFormatterProvider.Register(new CharacterGenderEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterGenderEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharacterGenderEntry>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Gender>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Gender>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterGenderEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.Gender);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharacterGenderEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Gender value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Gender>(out value2);
			}
			else
			{
				value2 = value.Gender;
				reader.ReadUnmanaged<Gender>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharacterGenderEntry), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Gender : Gender.Male);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Gender>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharacterGenderEntry(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterGenderEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Gender");
		writer.WriteUnmanaged(value.Gender);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterGenderEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Gender v = ((value != null) ? value.Gender : Gender.Male);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Gender")
				{
					reader.ReadUnmanaged<Gender>(out v);
					array[0] = true;
				}
			}
			else if (text == "Gender")
			{
				reader.ReadUnmanaged<Gender>(out v);
			}
		}
		_ = value;
		value = new CharacterGenderEntry(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharacterGenderEntry source = new CharacterGenderEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharacterGenderEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharacterGenderEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharacterGenderEntry>();
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
