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
public sealed class SpeedUpGameCommand : GameCommand, IMemoryPackable<SpeedUpGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SpeedUpGameCommand>
{
	[Preserve]
	private sealed class SpeedUpGameCommandFormatter : MemoryPackFormatter<SpeedUpGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public SpeedUpGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.DoSpeedUp();
	}

	static SpeedUpGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SpeedUpGameCommand",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SpeedUpGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SpeedUpGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SpeedUpGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SpeedUpGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SpeedUpGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref SpeedUpGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SpeedUpGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new SpeedUpGameCommand();
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SpeedUpGameCommand? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SpeedUpGameCommand? value)
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
			value = new SpeedUpGameCommand();
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SpeedUpGameCommand source = new SpeedUpGameCommand();
		result = Unsafe.As<SpeedUpGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SpeedUpGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SpeedUpGameCommand>();
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
