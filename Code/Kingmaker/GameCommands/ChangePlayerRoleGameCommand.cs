using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class ChangePlayerRoleGameCommand : GameCommand, IMemoryPackable<ChangePlayerRoleGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<ChangePlayerRoleGameCommand>
{
	[Preserve]
	private sealed class ChangePlayerRoleGameCommandFormatter : MemoryPackFormatter<ChangePlayerRoleGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly NetPlayerSerializable m_Player;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly string m_EntityId;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private ChangePlayerRoleGameCommand()
	{
	}

	[MemoryPackConstructor]
	private ChangePlayerRoleGameCommand(NetPlayerSerializable m_player, string m_entityId)
	{
		m_Player = m_player;
		m_EntityId = m_entityId;
	}

	public ChangePlayerRoleGameCommand(string entityId, NetPlayer player, bool enable)
		: this((NetPlayerSerializable)player, entityId)
	{
	}

	protected override void ExecuteInternal()
	{
		if (m_EntityId != null)
		{
			bool enable = true;
			Game.Instance.CoopData.PlayerRole.ForceSet(m_EntityId, (NetPlayer)m_Player, enable);
		}
	}

	static ChangePlayerRoleGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ChangePlayerRoleGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Player", typeof(NetPlayerSerializable)),
				new FieldInfo("m_EntityId", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ChangePlayerRoleGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ChangePlayerRoleGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ChangePlayerRoleGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ChangePlayerRoleGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ChangePlayerRoleGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_Player);
		writer.WriteString(value.m_EntityId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ChangePlayerRoleGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		NetPlayerSerializable value2;
		string entityId;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				entityId = reader.ReadString();
			}
			else
			{
				value2 = value.m_Player;
				entityId = value.m_EntityId;
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				entityId = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ChangePlayerRoleGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(NetPlayerSerializable);
				entityId = null;
			}
			else
			{
				value2 = value.m_Player;
				entityId = value.m_EntityId;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				if (memberCount != 1)
				{
					entityId = reader.ReadString();
					_ = 2;
				}
			}
			_ = value;
		}
		value = new ChangePlayerRoleGameCommand(value2, entityId);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ChangePlayerRoleGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Player");
		writer.WriteUnmanaged(value.m_Player);
		writer.WriteProperty("m_EntityId");
		writer.WriteString(value.m_EntityId);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ChangePlayerRoleGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		NetPlayerSerializable v;
		string entityId;
		if (value == null)
		{
			v = default(NetPlayerSerializable);
			entityId = null;
		}
		else
		{
			v = value.m_Player;
			entityId = value.m_EntityId;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Player"))
				{
					if (text == "m_EntityId")
					{
						entityId = reader.ReadString();
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<NetPlayerSerializable>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_Player"))
			{
				if (text == "m_EntityId")
				{
					entityId = reader.ReadString();
				}
			}
			else
			{
				reader.ReadUnmanaged<NetPlayerSerializable>(out v);
			}
		}
		_ = value;
		value = new ChangePlayerRoleGameCommand(v, entityId);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ChangePlayerRoleGameCommand source = new ChangePlayerRoleGameCommand();
		result = Unsafe.As<ChangePlayerRoleGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ChangePlayerRoleGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		NetPlayerSerializable value = m_Player;
		formatter.Field(0, "m_Player", ref value, state);
		string value2 = m_EntityId;
		formatter.StringField(1, "m_EntityId", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ChangePlayerRoleGameCommand>();
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
				Unsafe.AsRef(in m_Player) = formatter.ReadPackable<NetPlayerSerializable>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_EntityId) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
