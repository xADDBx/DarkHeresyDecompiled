using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.AreaLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class PartVeil : EntityPart<Area>, IHashable, IOwlPackable<PartVeil>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_Damage;

	private List<BlueprintPsykerRoot.PhenomenaData> m_CachedPhenomena;

	private List<BlueprintPsykerRoot.PhenomenaData> m_CachedPerils;

	private ActiveEncounter m_CachedEncounter;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartVeil",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Damage", typeof(int)),
			new FieldInfo("MinDamage", typeof(int)),
			new FieldInfo("PhenomenaStreak", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int MinDamage { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int PhenomenaStreak { get; set; }

	private BlueprintPsykerRoot PsykerRoot => ConfigRoot.Instance.PsykerRoot;

	public int PsychicPhenomenaChance => Math.Clamp(PsykerRoot.PhenomenaBaseChance + Damage * PsykerRoot.PhenomenaChancePerVeilDamage, 0, 100);

	public int PerilsOfTheWarpChance => Math.Clamp(PsykerRoot.PerilsBaseChance + PhenomenaStreak * PsykerRoot.PerilsChancePerConsecutivePhenomena, 0, 100);

	public int Damage
	{
		get
		{
			return m_Damage;
		}
		set
		{
			int delta = value - m_Damage;
			m_Damage = value;
			base.EventBus.RaiseEvent(delegate(IVeilDamageHandler h)
			{
				h.HandleVeilDamageChanged(delta, m_Damage);
			});
		}
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		MinDamage = base.Owner.Blueprint.StartVeilDamage;
		Damage = MinDamage;
	}

	public List<BlueprintPsykerRoot.PhenomenaData> GetResolvedPhenomena()
	{
		InvalidateCacheIfNeeded();
		return m_CachedPhenomena ?? (m_CachedPhenomena = ResolveList(PsykerRoot.PsychicPhenomena, base.Owner.Blueprint.PhenomenaOverride, ActiveEncounter.Current?.Blueprint.PhenomenaOverride));
	}

	public List<BlueprintPsykerRoot.PhenomenaData> GetResolvedPerils()
	{
		InvalidateCacheIfNeeded();
		return m_CachedPerils ?? (m_CachedPerils = ResolveList(PsykerRoot.PerilsOfTheWarp, base.Owner.Blueprint.PerilsOverride, ActiveEncounter.Current?.Blueprint.PerilsOverride));
	}

	public void InvalidatePhenomenaCache()
	{
		m_CachedPhenomena = null;
		m_CachedPerils = null;
		m_CachedEncounter = ActiveEncounter.Current;
	}

	private void InvalidateCacheIfNeeded()
	{
		if (m_CachedEncounter != ActiveEncounter.Current)
		{
			InvalidatePhenomenaCache();
		}
	}

	private static List<BlueprintPsykerRoot.PhenomenaData> ResolveList(BlueprintPsykerRoot.PhenomenaData[] rootList, PhenomenaListOverride areaOverride, PhenomenaListOverride encounterOverride)
	{
		List<BlueprintPsykerRoot.PhenomenaData> input = PhenomenaListResolver.ResolveFromRoot(rootList);
		input = areaOverride.Resolve(input);
		if (encounterOverride != null)
		{
			input = encounterOverride.Resolve(input);
		}
		return input;
	}

	public void UpdateDamage([NotNull] MechanicEntity initiator, UpdateVeilEventType @event, [CanBeNull] AbilityData ability = null, int customDamageDelta = 0)
	{
		RuleCalculateVeilDamage ruleCalculateVeilDamage = Rulebook.Trigger(new RuleCalculateVeilDamage(initiator, @event, ability, customDamageDelta));
		MinDamage = ruleCalculateVeilDamage.ResultMinDamage;
		Damage = ruleCalculateVeilDamage.ResultDamage;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Damage);
		int val2 = MinDamage;
		result.Append(ref val2);
		int val3 = PhenomenaStreak;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartVeil source = new PartVeil();
		result = Unsafe.As<PartVeil, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartVeil>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Damage", ref m_Damage, state);
		int value = MinDamage;
		formatter.UnmanagedField(1, "MinDamage", ref value, state);
		int value2 = PhenomenaStreak;
		formatter.UnmanagedField(2, "PhenomenaStreak", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartVeil>();
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
				m_Damage = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				MinDamage = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				PhenomenaStreak = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
