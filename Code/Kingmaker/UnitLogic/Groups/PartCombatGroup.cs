using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Groups;

[OwlPackable(OwlPackableMode.Generate)]
public class PartCombatGroup : BaseUnitPart, ICombatGroup, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IHashable, IOwlPackable<PartCombatGroup>
{
	public interface IOwner : IEntityPartOwner<PartCombatGroup>, IEntityPartOwner
	{
		PartCombatGroup CombatGroup { get; }
	}

	[JsonProperty]
	[OwlPackInclude]
	private string m_Id;

	private UnitGroup m_Group;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartCombatGroup",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Id", typeof(string))
		}
	};

	public UnitGroup Group => EnsureAndUpdateGroup();

	public UnitGroupMemory Memory => Group.Memory;

	public bool IsPlayerParty => Group.IsPlayerParty;

	public bool IsPeaceful => Group.IsPeaceful;

	public CountableFlag IsInCombat => Group.IsInCombat;

	public ReadonlyList<BlueprintFaction> AttackFactions => Group.AttackFactions;

	public bool IsInFogOfWar => Group.IsInFogOfWar;

	public int Count => Group.Count;

	public BaseUnitEntity this[int index] => Group[index];

	public string Id
	{
		get
		{
			InitIdIfNeed();
			return m_Id;
		}
		set
		{
			if (!(m_Id == value))
			{
				Drop();
				m_Id = value;
			}
		}
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		Drop();
	}

	public bool IsEnemy([CanBeNull] UnitGroup group)
	{
		if (group != null)
		{
			return Group.IsEnemy(group);
		}
		return false;
	}

	public bool IsEnemy([CanBeNull] MechanicEntity entity)
	{
		if (entity != null)
		{
			return Group.IsEnemy(entity);
		}
		return false;
	}

	public bool IsAlly(IMechanicEntity targetEntity)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)targetEntity;
		string text = mechanicEntity.GetCombatGroupOptional()?.Id;
		PartFaction factionOptional = mechanicEntity.GetFactionOptional();
		if (!base.Owner.Faction.IsPlayer)
		{
			if (factionOptional != null && !IsEnemy(mechanicEntity))
			{
				return base.Owner.Faction.IsAlly(factionOptional);
			}
			return false;
		}
		if (!mechanicEntity.IsPlayerFaction && !(text == Id))
		{
			return (mechanicEntity.GetOptional<UnitPartSummonedMonster>()?.Summoner?.IsPlayerFaction).GetValueOrDefault();
		}
		return true;
	}

	public bool Any(Func<BaseUnitEntity, bool> pred)
	{
		return Group.Any(pred);
	}

	public bool All(Func<BaseUnitEntity, bool> pred)
	{
		return Group.All(pred);
	}

	public IEnumerable<T> Select<T>(Func<BaseUnitEntity, T> select)
	{
		return Group.Select(select);
	}

	public void ForEach(Action<BaseUnitEntity> action)
	{
		Group.ForEach(action);
	}

	public bool HasLOS(BaseUnitEntity unit)
	{
		return Group.HasLOS(unit);
	}

	public void UpdateAttackFactionsCache()
	{
		Group.UpdateAttackFactionsCache();
	}

	public void ResetFactionSet()
	{
		Group.ResetFactionSet();
	}

	public UnitGroupEnumerator GetEnumerator()
	{
		return Group.GetEnumerator();
	}

	public bool CanAttack(MechanicEntity entity)
	{
		if (!entity.IsNeutral)
		{
			return Group.IsEnemy(entity);
		}
		return true;
	}

	public UnitGroup EnsureAndUpdateGroup()
	{
		if (m_Group != null && !m_Group.IsFake && !m_Group.Disposed)
		{
			return m_Group;
		}
		if (base.Owner.HoldingState != null && base.Owner.IsInGame)
		{
			m_Group = Game.Instance.UnitGroups.GetGroup(Id);
			m_Group.Add(base.Owner);
		}
		else if (m_Group == null)
		{
			m_Group = new UnitGroup(null);
			m_Group.Add(base.Owner);
		}
		return m_Group;
	}

	protected override void OnAttach()
	{
		InitIdIfNeed();
	}

	protected override void OnDetach()
	{
		Drop();
	}

	public void Drop()
	{
		m_Group?.Remove(base.Owner);
		m_Group = null;
	}

	protected override void OnDidPostLoad()
	{
		InitIdIfNeed();
	}

	private void InitIdIfNeed()
	{
		if (string.IsNullOrEmpty(m_Id))
		{
			m_Id = (base.Owner.Faction.IsPlayer ? "<directly-controllable-unit>" : base.Owner.UniqueId);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_Id);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartCombatGroup source = new PartCombatGroup();
		result = Unsafe.As<PartCombatGroup, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartCombatGroup>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "m_Id", ref m_Id, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartCombatGroup>();
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
				m_Id = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
