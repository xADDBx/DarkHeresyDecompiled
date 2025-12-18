using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class UnitAttackFactions : IEnumerable<BlueprintFaction>, IEnumerable, IHashable, IOwlPackable, IOwlPackable<UnitAttackFactions>
{
	private BaseUnitEntity m_Owner;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitAttackFactions",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Factions", typeof(HashSet<BlueprintFaction>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private HashSet<BlueprintFaction> m_Factions { get; set; } = new HashSet<BlueprintFaction>();


	public bool IsPlayerEnemy { get; private set; }

	[JsonConstructor]
	public UnitAttackFactions(BaseUnitEntity owner)
	{
		m_Owner = owner;
	}

	protected UnitAttackFactions()
	{
	}

	public HashSet<BlueprintFaction>.Enumerator GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	IEnumerator<BlueprintFaction> IEnumerable<BlueprintFaction>.GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	public void Add(BlueprintFaction faction)
	{
		m_Factions.Add(faction);
		UpdateIsPlayerEnemy();
	}

	public void Remove(BlueprintFaction faction)
	{
		m_Factions.Remove(faction);
		UpdateIsPlayerEnemy();
	}

	public void UnionWith(IEnumerable<BlueprintFaction> faction)
	{
		m_Factions.UnionWith(faction);
		UpdateIsPlayerEnemy();
	}

	public void Match(IEnumerable<BlueprintFaction> faction)
	{
		m_Factions.Clear();
		UnionWith(faction);
	}

	public bool Contains(BlueprintFaction faction)
	{
		return m_Factions.Contains(faction);
	}

	public void Clear()
	{
		m_Factions.Clear();
		UpdateIsPlayerEnemy();
	}

	private void UpdateIsPlayerEnemy()
	{
		IsPlayerEnemy = m_Owner.Faction.EnemyForEveryone || m_Factions.Contains(ConfigRoot.Instance.SystemMechanics.PlayerFaction);
		if (m_Owner.GetOptional<PartCombatGroup>()?.Owner != null)
		{
			m_Owner.GetOptional<PartCombatGroup>()?.UpdateAttackFactionsCache();
		}
		if (m_Owner.IsInitialized)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)m_Owner, (Action<IUnitChangeAttackFactionsHandler>)delegate(IUnitChangeAttackFactionsHandler h)
			{
				h.HandleUnitChangeAttackFactions(m_Owner);
			}, isCheckRuntime: true);
		}
	}

	public void PrePostLoad(BaseUnitEntity owner)
	{
		m_Owner = owner;
		UpdateIsPlayerEnemy();
	}

	public void PostLoad()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<BlueprintFaction> factions = m_Factions;
		if (factions != null)
		{
			int num = 0;
			foreach (BlueprintFaction item in factions)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitAttackFactions source = new UnitAttackFactions();
		result = Unsafe.As<UnitAttackFactions, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitAttackFactions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		HashSet<BlueprintFaction> value = m_Factions;
		formatter.Field(0, "m_Factions", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitAttackFactions>();
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
				m_Factions = formatter.ReadPackable<HashSet<BlueprintFaction>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
