using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PingEntityGameCommand : GameCommand, IMemoryPackable<PingEntityGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PingEntityGameCommand>
{
	[Preserve]
	private sealed class PingEntityGameCommandFormatter : MemoryPackFormatter<PingEntityGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef m_EntityRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingEntityGameCommand()
	{
	}

	[MemoryPackConstructor]
	private PingEntityGameCommand(EntityRef m_entityRef)
	{
		m_EntityRef = m_entityRef;
	}

	public PingEntityGameCommand([NotNull] Entity entity)
		: this(entity.Ref)
	{
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingEntityLocally(playerOrEmpty, m_EntityRef);
	}

	static PingEntityGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PingEntityGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_EntityRef", typeof(EntityRef))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingEntityGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingEntityGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingEntityGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingEntityGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingEntityGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_EntityRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingEntityGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef>();
			}
			else
			{
				value2 = value.m_EntityRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingEntityGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_EntityRef : default(EntityRef));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PingEntityGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PingEntityGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_EntityRef");
		writer.WritePackable(value.m_EntityRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PingEntityGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef val = ((value != null) ? value.m_EntityRef : default(EntityRef));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_EntityRef")
				{
					val = reader.ReadPackable<EntityRef>();
					array[0] = true;
				}
			}
			else if (text == "m_EntityRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new PingEntityGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingEntityGameCommand source = new PingEntityGameCommand();
		result = Unsafe.As<PingEntityGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingEntityGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_EntityRef", ref m_EntityRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingEntityGameCommand>();
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
				m_EntityRef = formatter.ReadPackable<EntityRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
