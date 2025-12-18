using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartyStrategistManager : EntityPart<Player>, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>, IAreaActivationHandler, IAreaEffectForceEndHandler, IHashable, IOwlPackable<PartyStrategistManager>
{
	private readonly Dictionary<StrategistTacticsAreaEffectType, AreaEffectEntity> m_StrategistAreaEffectEntities = new Dictionary<StrategistTacticsAreaEffectType, AreaEffectEntity>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartyStrategistManager",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool IsCastRestricted { get; set; }

	public bool AllowFirstRoundRule { get; set; } = true;


	public bool IsAlreadyCastedThisTurn(AbilityData ability)
	{
		StrategistTacticsAreaEffectType? strategistTacticsAreaEffectType = ability.Blueprint.ElementsArray.OfType<ContextActionSpawnAreaEffect>().FirstOrDefault()?.AreaEffect.TacticsAreaEffectType;
		if (strategistTacticsAreaEffectType.HasValue)
		{
			return m_StrategistAreaEffectEntities.ContainsKey(strategistTacticsAreaEffectType.Value);
		}
		return false;
	}

	public int GetTacticsZoneCount()
	{
		return m_StrategistAreaEffectEntities.Count((KeyValuePair<StrategistTacticsAreaEffectType, AreaEffectEntity> p) => !p.Value.ForceEnded);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			m_StrategistAreaEffectEntities.Clear();
			return;
		}
		IsCastRestricted = false;
		AllowFirstRoundRule = true;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			IsCastRestricted = false;
			AllowFirstRoundRule = true;
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		IsCastRestricted = false;
	}

	public void HandleAreaEffectSpawned()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity && areaEffectEntity.Blueprint.IsStrategistAbility)
		{
			IsCastRestricted = true;
			StrategistTacticsAreaEffectType tacticsAreaEffectType = areaEffectEntity.Blueprint.TacticsAreaEffectType;
			if (m_StrategistAreaEffectEntities.ContainsKey(tacticsAreaEffectType))
			{
				m_StrategistAreaEffectEntities[tacticsAreaEffectType].ForceEnd();
			}
			m_StrategistAreaEffectEntities[tacticsAreaEffectType] = areaEffectEntity;
		}
	}

	public void HandleAreaEffectForceEndRequested()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity && areaEffectEntity.Blueprint.IsStrategistAbility)
		{
			m_StrategistAreaEffectEntities.Remove(areaEffectEntity.Blueprint.TacticsAreaEffectType);
		}
	}

	public void HandleAreaEffectDestroyed()
	{
	}

	public void OnAreaActivated()
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint.IsStrategistAbility)
			{
				StrategistTacticsAreaEffectType tacticsAreaEffectType = areaEffect.Blueprint.TacticsAreaEffectType;
				m_StrategistAreaEffectEntities[tacticsAreaEffectType] = areaEffect;
			}
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
		PartyStrategistManager source = new PartyStrategistManager();
		result = Unsafe.As<PartyStrategistManager, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyStrategistManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyStrategistManager>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
