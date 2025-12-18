using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartEncounterMoraleVictoryWatcher : MechanicEntityPart<ActiveEncounter>, IGlobalRulebookHandler<RulePerformMoraleChange>, IRulebookHandler<RulePerformMoraleChange>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, IHashable, IOwlPackable<PartEncounterMoraleVictoryWatcher>
{
	private HashSet<AbilityExecutionContext> _activeAbilityExecutionContexts = new HashSet<AbilityExecutionContext>();

	private bool _isVictoryValidByLastPlayerAction;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEncounterMoraleVictoryWatcher",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool IsMoraleVictoryAllowed => _isVictoryValidByLastPlayerAction;

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		_activeAbilityExecutionContexts.Add(context);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		_activeAbilityExecutionContexts.Remove(context);
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventAboutToTrigger(RulePerformMoraleChange evt)
	{
	}

	void IRulebookHandler<RulePerformMoraleChange>.OnEventDidTrigger(RulePerformMoraleChange evt)
	{
		if (Game.Instance.Controllers.TurnController.IsPlayerTurn && !_isVictoryValidByLastPlayerAction && base.Owner.IsValidByDefaultMoraleConditions())
		{
			bool flag = evt.TargetUnit.IsPlayerEnemy && evt.ResultDeltaRaw < 0;
			if ((!evt.TargetUnit.IsPlayerEnemy && evt.ResultDelta > 0 && evt.EventType == MoraleEventType.EnemyDeath) || flag)
			{
				_isVictoryValidByLastPlayerAction = _activeAbilityExecutionContexts.Count > 0;
			}
		}
	}

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage evt)
	{
		if (Game.Instance.Controllers.TurnController.IsPlayerTurn && !_isVictoryValidByLastPlayerAction && base.Owner.IsValidByDefaultMoraleConditions())
		{
			bool isVictoryValidByLastPlayerAction = false;
			BaseUnitEntity targetUnit = evt.TargetUnit;
			if (targetUnit != null && !targetUnit.IsInPlayerParty && evt.ResultDamage.ResultDamageValue > 0)
			{
				isVictoryValidByLastPlayerAction = true;
			}
			_isVictoryValidByLastPlayerAction = isVictoryValidByLastPlayerAction;
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
		PartEncounterMoraleVictoryWatcher source = new PartEncounterMoraleVictoryWatcher();
		result = Unsafe.As<PartEncounterMoraleVictoryWatcher, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEncounterMoraleVictoryWatcher>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEncounterMoraleVictoryWatcher>();
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
