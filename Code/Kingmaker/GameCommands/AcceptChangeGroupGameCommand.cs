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
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class AcceptChangeGroupGameCommand : GameCommand, IOwlPackable<AcceptChangeGroupGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private List<UnitReference> m_PartyCharacters;

	[JsonProperty]
	[OwlPackInclude]
	private List<UnitReference> m_RemoteCharacters;

	[JsonProperty]
	[OwlPackInclude]
	private List<BlueprintUnitReference> m_RequiredCharacters;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_ReInitPartyCharacters;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
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

	public override bool IsSynchronized => true;

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
