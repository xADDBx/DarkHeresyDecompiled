using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Concentration.Events;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Concentration;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Gameplay.Parts.PartConcentration")]
public class PartConcentration : MechanicEntityPart, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IMoralePhaseHandler<EntitySubscriber>, IMoralePhaseHandler, ISubscriber<IMechanicEntity>, IEventTag<IMoralePhaseHandler, EntitySubscriber>, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IApplyAbilityEffectHandler, IHashable, IOwlPackable<PartConcentration>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<Buff> _buff;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartConcentration",
		OldNames = new string[1] { "Kingmaker.Gameplay.Parts.PartConcentration" },
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_buff", typeof(EntityFactRef<Buff>))
		}
	};

	public TargetWrapper Target { get; private set; }

	[CanBeNull]
	public Buff Buff => _buff;

	public void Add(Buff buff, ConcentrationLogic _)
	{
		Buff?.MarkExpired();
		_buff = buff;
		Target = buff.Context.ClickedTarget;
	}

	public void Remove(Buff buff, ConcentrationLogic _)
	{
		if (Buff == buff)
		{
			Buff?.MarkExpired();
			_buff = null;
			Target = null;
			RemoveSelf();
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (Buff != null && Buff.GetComponent<ConcentrationLogic>() == null)
		{
			Buff.MarkExpired();
			RemoveSelf();
		}
	}

	public void Break([CanBeNull] MechanicEntity reason)
	{
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IConcentrationBrokenHandler>)delegate(IConcentrationBrokenHandler h)
		{
			h.HandleConcentrationBroken(reason);
		}, isCheckRuntime: true);
		Buff?.MarkExpired();
		_buff = null;
		Target = null;
		RemoveSelf();
	}

	void IRulebookHandler<RulePerformAttack>.OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	void IRulebookHandler<RulePerformAttack>.OnEventDidTrigger(RulePerformAttack evt)
	{
	}

	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		if (phase == MoralePhaseType.Broken)
		{
			Break(EventInvokerExtensions.MechanicEntity);
		}
	}

	void IEntityGainFactHandler.HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Blueprint is BlueprintBuff { IsHardCrowdControl: not false })
		{
			Break(fact.MaybeContext?.SourceCaster);
		}
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	void IApplyAbilityEffectHandler.OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (target.Target.Entity == base.Owner && context.Ability.Blueprint.BreakConcentration() && !(base.Owner?.Features.SteadyConcentration))
		{
			Break(context.SourceCaster);
		}
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityFactRef<Buff> obj = _buff;
		Hash128 val2 = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartConcentration source = new PartConcentration();
		result = Unsafe.As<PartConcentration, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartConcentration>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_buff", ref _buff, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartConcentration>();
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
				_buff = formatter.ReadPackable<EntityFactRef<Buff>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
