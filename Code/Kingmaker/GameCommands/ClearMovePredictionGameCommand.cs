using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Units;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class ClearMovePredictionGameCommand : GameCommandWithSynchronized, IMemoryPackable<ClearMovePredictionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<ClearMovePredictionGameCommand>
{
	[Preserve]
	private sealed class ClearMovePredictionGameCommandFormatter : MemoryPackFormatter<ClearMovePredictionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	[JsonConstructor]
	[MemoryPackConstructor]
	private ClearMovePredictionGameCommand()
	{
	}

	public ClearMovePredictionGameCommand(bool isSynchronized)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		UnitCommandsRunner.CancelMoveCommandLocal();
	}

	static ClearMovePredictionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ClearMovePredictionGameCommand",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearMovePredictionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearMovePredictionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearMovePredictionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearMovePredictionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClearMovePredictionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearMovePredictionGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearMovePredictionGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ClearMovePredictionGameCommand();
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ClearMovePredictionGameCommand? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ClearMovePredictionGameCommand? value)
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
			value = new ClearMovePredictionGameCommand();
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ClearMovePredictionGameCommand source = new ClearMovePredictionGameCommand();
		result = Unsafe.As<ClearMovePredictionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ClearMovePredictionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ClearMovePredictionGameCommand>();
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
