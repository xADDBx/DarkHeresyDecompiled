using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharacterNameEntry : IMemoryPackable<CharacterNameEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<CharacterNameEntry>
{
	[Preserve]
	private sealed class CharacterNameEntryFormatter : MemoryPackFormatter<CharacterNameEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterNameEntry value)
		{
			CharacterNameEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterNameEntry value)
		{
			CharacterNameEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharacterNameEntry value)
		{
			CharacterNameEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterNameEntry value)
		{
			CharacterNameEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly string Name;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public CharacterNameEntry([NotNull] string name)
	{
		Name = name;
	}

	public CharacterNameEntry(OwlPackConstructorParameter _)
	{
	}

	static CharacterNameEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharacterNameEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterNameEntry>())
		{
			MemoryPackFormatterProvider.Register(new CharacterNameEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharacterNameEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharacterNameEntry>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharacterNameEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.Name);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharacterNameEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string name;
		if (memberCount == 1)
		{
			if (value == null)
			{
				name = reader.ReadString();
			}
			else
			{
				name = value.Name;
				name = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharacterNameEntry), 1, memberCount);
				return;
			}
			name = ((value != null) ? value.Name : null);
			if (memberCount != 0)
			{
				name = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new CharacterNameEntry(name);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharacterNameEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Name");
		writer.WriteString(value.Name);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharacterNameEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string name = ((value != null) ? value.Name : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Name")
				{
					name = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text == "Name")
			{
				name = reader.ReadString();
			}
		}
		_ = value;
		value = new CharacterNameEntry(name);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharacterNameEntry source = new CharacterNameEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharacterNameEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharacterNameEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharacterNameEntry>();
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
