using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PingPositionGameCommand : GameCommand, IMemoryPackable<PingPositionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PingPositionGameCommand>
{
	[Preserve]
	private sealed class PingPositionGameCommandFormatter : MemoryPackFormatter<PingPositionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private Vector3 m_Position;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingPositionGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingPositionGameCommand(Vector3 m_position)
	{
		m_Position = m_position;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingPositionLocally(playerOrEmpty, m_Position);
	}

	static PingPositionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PingPositionGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Position", typeof(Vector3))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingPositionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingPositionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingPositionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingPositionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingPositionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Position);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingPositionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Vector3 value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
			}
			else
			{
				value2 = value.m_Position;
				reader.ReadUnmanaged<Vector3>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingPositionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Position : default(Vector3));
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PingPositionGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PingPositionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Position");
		writer.WriteUnmanaged(value.m_Position);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PingPositionGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Vector3 v = ((value != null) ? value.m_Position : default(Vector3));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Position")
				{
					reader.ReadUnmanaged<Vector3>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Position")
			{
				reader.ReadUnmanaged<Vector3>(out v);
			}
		}
		_ = value;
		value = new PingPositionGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingPositionGameCommand source = new PingPositionGameCommand();
		result = Unsafe.As<PingPositionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingPositionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Position", ref m_Position, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingPositionGameCommand>();
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
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
