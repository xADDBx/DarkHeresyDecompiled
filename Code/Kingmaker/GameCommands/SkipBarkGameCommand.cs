using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SkipBarkGameCommand : GameCommand, IMemoryPackable<SkipBarkGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SkipBarkGameCommand>
{
	[Preserve]
	private sealed class SkipBarkGameCommandFormatter : MemoryPackFormatter<SkipBarkGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		CutsceneController.SkipBarkBanter();
	}

	static SkipBarkGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SkipBarkGameCommand",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SkipBarkGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SkipBarkGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SkipBarkGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SkipBarkGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SkipBarkGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteObjectHeader(0);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SkipBarkGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		if (memberCount == 0)
		{
			if (value != null)
			{
				return;
			}
		}
		else
		{
			if (memberCount > 0)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SkipBarkGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new SkipBarkGameCommand();
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SkipBarkGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
		}
		else
		{
			writer.WriteEmptyObject();
		}
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SkipBarkGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		_ = value;
		_ = new bool[0];
		while (reader.ReadPropertyName() != null)
		{
			_ = value;
		}
		if (value == null)
		{
			value = new SkipBarkGameCommand();
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SkipBarkGameCommand source = new SkipBarkGameCommand();
		result = Unsafe.As<SkipBarkGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SkipBarkGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SkipBarkGameCommand>();
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
