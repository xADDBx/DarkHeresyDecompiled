using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class ClearPointerModeGameCommand : GameCommand, IMemoryPackable<ClearPointerModeGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<ClearPointerModeGameCommand>
{
	[Preserve]
	private sealed class ClearPointerModeGameCommandFormatter : MemoryPackFormatter<ClearPointerModeGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public ClearPointerModeGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
	}

	static ClearPointerModeGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ClearPointerModeGameCommand",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearPointerModeGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearPointerModeGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearPointerModeGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearPointerModeGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClearPointerModeGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearPointerModeGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearPointerModeGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ClearPointerModeGameCommand();
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ClearPointerModeGameCommand? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ClearPointerModeGameCommand? value)
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
			value = new ClearPointerModeGameCommand();
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ClearPointerModeGameCommand source = new ClearPointerModeGameCommand();
		result = Unsafe.As<ClearPointerModeGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ClearPointerModeGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ClearPointerModeGameCommand>();
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
