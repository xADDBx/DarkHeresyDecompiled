using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Reputation;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class ReputationState : Entity, IHashable, IOwlPackable<ReputationState>
{
	public const string ID = "reputation-state-id";

	public new static readonly EntityRef<ReputationState> Ref = new EntityRef<ReputationState>("reputation-state-id");

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<FactionType, ReputationDescription> _reputation = new Dictionary<FactionType, ReputationDescription>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ReputationState",
		OldNames = null,
		Fields = new FieldInfo[11]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("_reputation", typeof(Dictionary<FactionType, ReputationDescription>))
		}
	};

	public override bool NeedsView => false;

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	public ReputationState()
		: base("reputation-state-id", isInGame: true)
	{
	}

	private ReputationState(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public void AddRespect(FactionType faction, int value)
	{
		_reputation[faction] = _reputation.GetValueOrDefault(faction).AddRespect(value);
		Metrics.Reputation.Value(value).Type(ReputationType.Respect).Faction(faction)
			.CharacterLevel(Game.Instance.Player.PartyLevel)
			.Send();
		base.EventBus.RaiseEvent(delegate(IGainFactionReputationHandler h)
		{
			h.HandleGainFactionReputation(faction, ReputationType.Respect, value);
		});
	}

	public void AddFear(FactionType faction, int value)
	{
		_reputation[faction] = _reputation.GetValueOrDefault(faction).AddFear(value);
		Metrics.Reputation.Value(value).Type(ReputationType.Fear).Faction(faction)
			.CharacterLevel(Game.Instance.Player.PartyLevel)
			.Send();
		base.EventBus.RaiseEvent(delegate(IGainFactionReputationHandler h)
		{
			h.HandleGainFactionReputation(faction, ReputationType.Fear, value);
		});
	}

	public void Add(FactionType faction, ReputationType reputationType, int value)
	{
		switch (reputationType)
		{
		case ReputationType.Respect:
			AddRespect(faction, value);
			break;
		case ReputationType.Fear:
			AddFear(faction, value);
			break;
		default:
			throw new ArgumentOutOfRangeException("reputationType", reputationType, null);
		}
	}

	public ReputationDescription Get(FactionType faction)
	{
		return _reputation.GetValueOrDefault(faction);
	}

	public int Get(FactionType faction, ReputationType reputationType)
	{
		ReputationDescription reputationDescription = Get(faction);
		return reputationType switch
		{
			ReputationType.Respect => reputationDescription.Respect, 
			ReputationType.Fear => reputationDescription.Fear, 
			_ => throw new ArgumentOutOfRangeException("reputationType", reputationType, null), 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<FactionType, ReputationDescription> reputation = _reputation;
		if (reputation != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<FactionType, ReputationDescription> item in reputation)
			{
				Hash128 hash = default(Hash128);
				FactionType obj = item.Key;
				Hash128 val3 = UnmanagedHasher<FactionType>.GetHash128(ref obj);
				hash.Append(ref val3);
				ReputationDescription obj2 = item.Value;
				Hash128 val4 = UnmanagedHasher<ReputationDescription>.GetHash128(ref obj2);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ReputationState source = new ReputationState(default(OwlPackConstructorParameter));
		result = Unsafe.As<ReputationState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ReputationState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "_reputation", ref _reputation, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ReputationState>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				_reputation = formatter.ReadPackable<Dictionary<FactionType, ReputationDescription>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
