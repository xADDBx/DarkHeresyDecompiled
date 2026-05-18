using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class AcceptChangeGroupGameCommand : GameCommand, IMemoryPackable<AcceptChangeGroupGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AcceptChangeGroupGameCommand>
{
	[Preserve]
	private sealed class AcceptChangeGroupGameCommandFormatter : MemoryPackFormatter<AcceptChangeGroupGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<UnitReference> m_PartyCharacters;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<UnitReference> m_RemoteCharacters;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<BlueprintUnitReference> m_RequiredCharacters;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_ReInitPartyCharacters;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AcceptChangeGroupGameCommand()
	{
	}

	[JsonConstructor]
	public AcceptChangeGroupGameCommand([NotNull] List<UnitReference> partyCharacters, [NotNull] List<UnitReference> remoteCharacters, [NotNull] List<BlueprintUnitReference> requiredCharacters, bool reInitPartyCharacters)
	{
		m_PartyCharacters = partyCharacters;
		m_RemoteCharacters = remoteCharacters;
		m_RequiredCharacters = requiredCharacters;
		m_ReInitPartyCharacters = reInitPartyCharacters;
	}

	protected override void ExecuteInternal()
	{
		if (!CanChangeGroup())
		{
			return;
		}
		if (m_ReInitPartyCharacters)
		{
			Game.Instance.Player.ReInitPartyCharacters(m_PartyCharacters.ToList());
			EventBus.RaiseEvent(delegate(IAcceptChangeGroupHandler h)
			{
				h.HandleAcceptChangeGroup();
			});
			return;
		}
		foreach (UnitReference item in m_PartyCharacters.Where((UnitReference unitRef) => Game.Instance.Player.PartyAndPetsDetached.Contains(unitRef.ToBaseUnitEntity())))
		{
			Game.Instance.Player.AttachPartyMember(item.ToBaseUnitEntity());
		}
		foreach (UnitReference remoteCharacter in m_RemoteCharacters)
		{
			if (Game.Instance.Player.PartyCharacters.Contains(remoteCharacter))
			{
				Game.Instance.Player.DetachPartyMember(remoteCharacter.ToBaseUnitEntity());
			}
		}
		EventBus.RaiseEvent(delegate(IAcceptChangeGroupHandler h)
		{
			h.HandleAcceptChangeGroup();
		});
	}

	private bool CanChangeGroup()
	{
		if (!m_ReInitPartyCharacters)
		{
			if (m_PartyCharacters.Count > 0)
			{
				return m_RemoteCharacters.Count > 0;
			}
			return false;
		}
		if (m_RemoteCharacters.Any((UnitReference v) => MustBeInParty(v.ToBaseUnitEntity())))
		{
			return false;
		}
		return true;
	}

	private bool MustBeInParty(BaseUnitEntity character)
	{
		if (character != Game.Instance.Player.MainCharacterEntity && character.Blueprint.GetComponent<LockedCompanionComponent>() == null && !m_RequiredCharacters.Any((BlueprintUnitReference x) => x.Get() == character.Blueprint))
		{
			return PartPartyLock.IsLocked(character);
		}
		return true;
	}

	static AcceptChangeGroupGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AcceptChangeGroupGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_PartyCharacters", typeof(List<UnitReference>)),
				new FieldInfo("m_RemoteCharacters", typeof(List<UnitReference>)),
				new FieldInfo("m_RequiredCharacters", typeof(List<BlueprintUnitReference>)),
				new FieldInfo("m_ReInitPartyCharacters", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AcceptChangeGroupGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AcceptChangeGroupGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AcceptChangeGroupGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AcceptChangeGroupGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<UnitReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<UnitReference>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<BlueprintUnitReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BlueprintUnitReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AcceptChangeGroupGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		ListFormatter.SerializePackable(ref writer, value.m_PartyCharacters);
		ListFormatter.SerializePackable(ref writer, value.m_RemoteCharacters);
		ListFormatter.SerializePackable(ref writer, value.m_RequiredCharacters);
		writer.WriteUnmanaged(in value.m_ReInitPartyCharacters);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AcceptChangeGroupGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<UnitReference> value2;
		List<UnitReference> value3;
		List<BlueprintUnitReference> value4;
		bool value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.m_PartyCharacters;
				value3 = value.m_RemoteCharacters;
				value4 = value.m_RequiredCharacters;
				value5 = value.m_ReInitPartyCharacters;
				ListFormatter.DeserializePackable(ref reader, ref value2);
				ListFormatter.DeserializePackable(ref reader, ref value3);
				ListFormatter.DeserializePackable(ref reader, ref value4);
				reader.ReadUnmanaged<bool>(out value5);
				goto IL_00f8;
			}
			value2 = ListFormatter.DeserializePackable<UnitReference>(ref reader);
			value3 = ListFormatter.DeserializePackable<UnitReference>(ref reader);
			value4 = ListFormatter.DeserializePackable<BlueprintUnitReference>(ref reader);
			reader.ReadUnmanaged<bool>(out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AcceptChangeGroupGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = null;
				value5 = false;
			}
			else
			{
				value2 = value.m_PartyCharacters;
				value3 = value.m_RemoteCharacters;
				value4 = value.m_RequiredCharacters;
				value5 = value.m_ReInitPartyCharacters;
			}
			if (memberCount != 0)
			{
				ListFormatter.DeserializePackable(ref reader, ref value2);
				if (memberCount != 1)
				{
					ListFormatter.DeserializePackable(ref reader, ref value3);
					if (memberCount != 2)
					{
						ListFormatter.DeserializePackable(ref reader, ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f8;
			}
		}
		value = new AcceptChangeGroupGameCommand
		{
			m_PartyCharacters = value2,
			m_RemoteCharacters = value3,
			m_RequiredCharacters = value4,
			m_ReInitPartyCharacters = value5
		};
		return;
		IL_00f8:
		value.m_PartyCharacters = value2;
		value.m_RemoteCharacters = value3;
		value.m_RequiredCharacters = value4;
		value.m_ReInitPartyCharacters = value5;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AcceptChangeGroupGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_PartyCharacters");
		ListFormatter.SerializePackableJson(ref writer, value.m_PartyCharacters);
		writer.WriteProperty("m_RemoteCharacters");
		ListFormatter.SerializePackableJson(ref writer, value.m_RemoteCharacters);
		writer.WriteProperty("m_RequiredCharacters");
		ListFormatter.SerializePackableJson(ref writer, value.m_RequiredCharacters);
		writer.WriteProperty("m_ReInitPartyCharacters");
		writer.WriteUnmanaged(value.m_ReInitPartyCharacters);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AcceptChangeGroupGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		List<UnitReference> value2;
		List<UnitReference> value3;
		List<BlueprintUnitReference> value4;
		bool v;
		if (value == null)
		{
			value2 = null;
			value3 = null;
			value4 = null;
			v = false;
		}
		else
		{
			value2 = value.m_PartyCharacters;
			value3 = value.m_RemoteCharacters;
			value4 = value.m_RequiredCharacters;
			v = value.m_ReInitPartyCharacters;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_PartyCharacters":
					value2 = ListFormatter.DeserializePackableJson<UnitReference>(ref reader);
					array[0] = true;
					break;
				case "m_RemoteCharacters":
					value3 = ListFormatter.DeserializePackableJson<UnitReference>(ref reader);
					array[1] = true;
					break;
				case "m_RequiredCharacters":
					value4 = ListFormatter.DeserializePackableJson<BlueprintUnitReference>(ref reader);
					array[2] = true;
					break;
				case "m_ReInitPartyCharacters":
					reader.ReadUnmanaged<bool>(out v);
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_PartyCharacters":
					ListFormatter.DeserializePackableJson(ref reader, ref value2);
					break;
				case "m_RemoteCharacters":
					ListFormatter.DeserializePackableJson(ref reader, ref value3);
					break;
				case "m_RequiredCharacters":
					ListFormatter.DeserializePackableJson(ref reader, ref value4);
					break;
				case "m_ReInitPartyCharacters":
					reader.ReadUnmanaged<bool>(out v);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_PartyCharacters = value2;
			value.m_RemoteCharacters = value3;
			value.m_RequiredCharacters = value4;
			value.m_ReInitPartyCharacters = v;
		}
		else
		{
			value = new AcceptChangeGroupGameCommand
			{
				m_PartyCharacters = value2,
				m_RemoteCharacters = value3,
				m_RequiredCharacters = value4,
				m_ReInitPartyCharacters = v
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
		AcceptChangeGroupGameCommand source = new AcceptChangeGroupGameCommand();
		result = Unsafe.As<AcceptChangeGroupGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AcceptChangeGroupGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_PartyCharacters", ref m_PartyCharacters, state);
		formatter.Field(1, "m_RemoteCharacters", ref m_RemoteCharacters, state);
		formatter.Field(2, "m_RequiredCharacters", ref m_RequiredCharacters, state);
		formatter.UnmanagedField(3, "m_ReInitPartyCharacters", ref m_ReInitPartyCharacters, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AcceptChangeGroupGameCommand>();
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
				m_PartyCharacters = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			case 1:
				m_RemoteCharacters = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			case 2:
				m_RequiredCharacters = formatter.ReadPackable<List<BlueprintUnitReference>>(state);
				break;
			case 3:
				m_ReInitPartyCharacters = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
