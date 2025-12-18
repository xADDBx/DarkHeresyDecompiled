using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class ChangePlayerRoleGameCommand : GameCommand, IOwlPackable<ChangePlayerRoleGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly NetPlayerSerializable m_Player;

	[JsonProperty]
	[OwlPackInclude]
	private readonly string m_EntityId;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ChangePlayerRoleGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Player", typeof(NetPlayerSerializable)),
			new FieldInfo("m_EntityId", typeof(string))
		}
	};

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private ChangePlayerRoleGameCommand()
	{
	}

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
