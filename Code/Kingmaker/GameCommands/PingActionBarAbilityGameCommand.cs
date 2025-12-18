using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PingActionBarAbilityGameCommand : GameCommand, IOwlPackable<PingActionBarAbilityGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_KeyName;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef m_CharacterEntityRef;

	[JsonProperty]
	[OwlPackInclude]
	private int m_SlotIndex;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
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

	public override bool IsSynchronized => true;

	[JsonConstructor]
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
