using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartFaction : BaseUnitPart, IEquatable<PartFaction>, IHashable, IOwlPackable<PartFaction>
{
	public interface IOwner : IEntityPartOwner<PartFaction>, IEntityPartOwner
	{
		PartFaction Faction { get; }
	}

	private bool? m_CachedIsPlayerFaction;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartFaction",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Blueprint", typeof(BlueprintFaction)),
			new FieldInfo("AttackFactions", typeof(UnitAttackFactions))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintFaction Blueprint { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public UnitAttackFactions AttackFactions { get; private set; }

	public bool IsPlayer
	{
		get
		{
			bool valueOrDefault = m_CachedIsPlayerFaction.GetValueOrDefault();
			if (!m_CachedIsPlayerFaction.HasValue)
			{
				valueOrDefault = Blueprint == ConfigRoot.Instance.SystemMechanics.PlayerFaction;
				m_CachedIsPlayerFaction = valueOrDefault;
			}
			return m_CachedIsPlayerFaction.Value;
		}
	}

	public bool IsPlayerEnemy
	{
		get
		{
			if (!IsPlayer && (EnemyForEveryone || base.Owner.CombatGroup.AttackFactions.Contains(ConfigRoot.Instance.SystemMechanics.PlayerFaction)))
			{
				return AttackFactions.IsPlayerEnemy;
			}
			return false;
		}
	}

	public bool Peaceful => Blueprint.Peaceful;

	public bool AlwaysEnemy => Blueprint.AlwaysEnemy;

	public bool EnemyForEveryone => Blueprint.EnemyForEveryone;

	public bool Neutral => Blueprint.Neutral;

	public bool IsDirectlyControllable => Blueprint.IsDirectlyControllable;

	public bool NeverJoinCombat => Blueprint.NeverJoinCombat;

	protected override void OnAttach()
	{
		AttackFactions = new UnitAttackFactions(base.Owner);
		Set(base.Owner.Blueprint.Faction, initializingNow: true);
		AttackFactions.UnionWith(base.Owner.Blueprint.AttackFactions);
	}

	public void Set(BlueprintFaction faction)
	{
		Set(faction, initializingNow: false);
	}

	private void Set(BlueprintFaction faction, bool initializingNow)
	{
		if (Blueprint == faction)
		{
			AttackFactions.Match(faction.AttackFactions);
			return;
		}
		Blueprint = faction;
		AttackFactions.Match(faction.AttackFactions);
		m_CachedIsPlayerFaction = null;
		if (!initializingNow)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFactionHandler>)delegate(IUnitFactionHandler h)
			{
				h.HandleFactionChanged();
			}, isCheckRuntime: true);
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			UnitPartSummonedMonster optional = allUnit.GetOptional<UnitPartSummonedMonster>();
			if (optional != null && optional.Summoner == base.Owner)
			{
				optional.Owner.GetOptional<PartFaction>()?.Set(faction);
			}
		}
	}

	public bool IsAlly(BlueprintFaction other)
	{
		return Blueprint.IsAlly(other);
	}

	public bool IsAlly(PartFaction other)
	{
		return Blueprint.IsAlly(other.Blueprint);
	}

	protected override void OnPrePostLoad()
	{
		AttackFactions.PrePostLoad(base.Owner);
	}

	protected override void OnPostLoad()
	{
		AttackFactions.PostLoad();
		if (AttackFactions.Contains(Blueprint))
		{
			AttackFactions.Remove(Blueprint);
			PFLog.Default.Error("AttackFactions of unit can't contains unit's own Faction, fixed ({0})", this);
		}
	}

	public static bool operator ==(PartFaction f1, PartFaction f2)
	{
		return f1?.Blueprint == f2?.Blueprint;
	}

	public static bool operator !=(PartFaction f1, PartFaction f2)
	{
		return !(f1 == f2);
	}

	public override int GetHashCode()
	{
		throw new Exception("Dont use unit parts as keys");
	}

	public bool Equals(PartFaction other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((PartFaction)obj);
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<UnitAttackFactions>.GetHash128(AttackFactions);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartFaction source = new PartFaction();
		result = Unsafe.As<PartFaction, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartFaction>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintFaction value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		UnitAttackFactions value2 = AttackFactions;
		formatter.Field(1, "AttackFactions", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartFaction>();
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
				Blueprint = formatter.ReadPackable<BlueprintFaction>(state);
				break;
			case 1:
				AttackFactions = formatter.ReadPackable<UnitAttackFactions>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
