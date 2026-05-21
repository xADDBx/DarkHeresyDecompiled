using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartEncounterMetrics : EntityPart<ActiveEncounter>, IHashable, IOwlPackable<PartEncounterMetrics>
{
	[OwlPackInclude]
	public int DamageToParty;

	[OwlPackInclude]
	public int DamageToEnemies;

	private readonly Dictionary<string, Dictionary<string, int>> _abilityCasts = new Dictionary<string, Dictionary<string, int>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEncounterMetrics",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("DamageToParty", typeof(int)),
			new FieldInfo("DamageToEnemies", typeof(int))
		}
	};

	public void HandleIncomingDamage(BaseUnitEntity target, int amount)
	{
		if (target != null)
		{
			if (target.IsPlayerFaction)
			{
				DamageToParty += amount;
			}
			else
			{
				DamageToEnemies += amount;
			}
		}
	}

	public void HandleCastAbility(string casterId, string abilityId)
	{
		int value2;
		if (!_abilityCasts.TryGetValue(casterId, out var value))
		{
			value = new Dictionary<string, int> { { abilityId, 1 } };
			_abilityCasts.Add(casterId, value);
		}
		else if (value.TryGetValue(abilityId, out value2))
		{
			value[abilityId] = value2 + 1;
		}
		else
		{
			value.Add(abilityId, 1);
		}
	}

	public int GetAbilityCastCount(string casterId, string abilityId)
	{
		if (!_abilityCasts.TryGetValue(casterId, out var value))
		{
			return 0;
		}
		return value[abilityId];
	}

	public void Reset()
	{
		DamageToParty = 0;
		DamageToEnemies = 0;
		foreach (KeyValuePair<string, Dictionary<string, int>> abilityCast in _abilityCasts)
		{
			abilityCast.Value.Clear();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartEncounterMetrics source = new PartEncounterMetrics();
		result = Unsafe.As<PartEncounterMetrics, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEncounterMetrics>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "DamageToParty", ref DamageToParty, state);
		formatter.UnmanagedField(1, "DamageToEnemies", ref DamageToEnemies, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEncounterMetrics>();
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
				DamageToParty = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				DamageToEnemies = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
