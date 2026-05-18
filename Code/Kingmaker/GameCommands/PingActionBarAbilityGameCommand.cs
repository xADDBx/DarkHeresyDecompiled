using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
public sealed class PingActionBarAbilityGameCommand : GameCommand, IMemoryPackable<PingActionBarAbilityGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PingActionBarAbilityGameCommand>
{
	[Preserve]
	private sealed class PingActionBarAbilityGameCommandFormatter : MemoryPackFormatter<PingActionBarAbilityGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_KeyName;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef m_CharacterEntityRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_SlotIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private PingActionBarAbilityGameCommand()
	{
	}

	public PingActionBarAbilityGameCommand(string m_keyName, Entity m_characterEntityRef, int m_slotIndex)
		: this()
	{
		m_KeyName = m_keyName;
		m_CharacterEntityRef = m_characterEntityRef.Ref;
		m_SlotIndex = m_slotIndex;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingActionBarAbilityLocally(playerOrEmpty, m_KeyName, m_CharacterEntityRef, m_SlotIndex);
	}

	static PingActionBarAbilityGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PingActionBarAbilityGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_KeyName", typeof(string)),
				new FieldInfo("m_CharacterEntityRef", typeof(EntityRef)),
				new FieldInfo("m_SlotIndex", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingActionBarAbilityGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingActionBarAbilityGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingActionBarAbilityGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingActionBarAbilityGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PingActionBarAbilityGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteString(value.m_KeyName);
		writer.WritePackable(in value.m_CharacterEntityRef);
		writer.WriteUnmanaged(in value.m_SlotIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingActionBarAbilityGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		int value3;
		string keyName;
		if (memberCount == 3)
		{
			if (value != null)
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
				keyName = reader.ReadString();
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				goto IL_00cc;
			}
			keyName = reader.ReadString();
			value2 = reader.ReadPackable<EntityRef>();
			reader.ReadUnmanaged<int>(out value3);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingActionBarAbilityGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				keyName = null;
				value2 = default(EntityRef);
				value3 = 0;
			}
			else
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
			}
			if (memberCount != 0)
			{
				keyName = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value2);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value3);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00cc;
			}
		}
		value = new PingActionBarAbilityGameCommand
		{
			m_KeyName = keyName,
			m_CharacterEntityRef = value2,
			m_SlotIndex = value3
		};
		return;
		IL_00cc:
		value.m_KeyName = keyName;
		value.m_CharacterEntityRef = value2;
		value.m_SlotIndex = value3;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PingActionBarAbilityGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_KeyName");
		writer.WriteString(value.m_KeyName);
		writer.WriteProperty("m_CharacterEntityRef");
		writer.WritePackable(value.m_CharacterEntityRef);
		writer.WriteProperty("m_SlotIndex");
		writer.WriteUnmanaged(value.m_SlotIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PingActionBarAbilityGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string keyName;
		EntityRef val;
		int v;
		if (value == null)
		{
			keyName = null;
			val = default(EntityRef);
			v = 0;
		}
		else
		{
			keyName = value.m_KeyName;
			val = value.m_CharacterEntityRef;
			v = value.m_SlotIndex;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_KeyName":
					keyName = reader.ReadString();
					array[0] = true;
					break;
				case "m_CharacterEntityRef":
					val = reader.ReadPackable<EntityRef>();
					array[1] = true;
					break;
				case "m_SlotIndex":
					reader.ReadUnmanaged<int>(out v);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_KeyName":
					keyName = reader.ReadString();
					break;
				case "m_CharacterEntityRef":
					reader.ReadPackable(ref val);
					break;
				case "m_SlotIndex":
					reader.ReadUnmanaged<int>(out v);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_KeyName = keyName;
			value.m_CharacterEntityRef = val;
			value.m_SlotIndex = v;
		}
		else
		{
			value = new PingActionBarAbilityGameCommand
			{
				m_KeyName = keyName,
				m_CharacterEntityRef = val,
				m_SlotIndex = v
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PingActionBarAbilityGameCommand source = new PingActionBarAbilityGameCommand();
		result = Unsafe.As<PingActionBarAbilityGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PingActionBarAbilityGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_KeyName", ref m_KeyName, state);
		formatter.Field(1, "m_CharacterEntityRef", ref m_CharacterEntityRef, state);
		formatter.UnmanagedField(2, "m_SlotIndex", ref m_SlotIndex, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PingActionBarAbilityGameCommand>();
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
				m_KeyName = formatter.ReadString(state);
				break;
			case 1:
				m_CharacterEntityRef = formatter.ReadPackable<EntityRef>(state);
				break;
			case 2:
				m_SlotIndex = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
