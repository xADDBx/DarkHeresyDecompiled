using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilityRestrictions : BaseUnitPart, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, IInterruptTurnEndHandler<EntitySubscriber>, IInterruptTurnEndHandler, IEventTag<IInterruptTurnEndHandler, EntitySubscriber>, IHashable, IOwlPackable<PartAbilityRestrictions>
{
	private readonly struct Entry
	{
		public readonly EntityFactRef SourceFact;

		public readonly BlueprintComponentReference SourceComponent;

		public readonly RestrictionCalculator AbilityFilter;

		public readonly RestrictionCalculator TargetRestriction;

		public readonly bool ForbidAbility;

		public Entry(EntityFactRef sourceFact, BlueprintComponentReference sourceComponent, RestrictionCalculator abilityFilter, RestrictionCalculator targetRestriction, bool forbidAbility)
		{
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
			AbilityFilter = abilityFilter;
			TargetRestriction = targetRestriction;
			ForbidAbility = forbidAbility;
		}

		public bool IsRestrictionPassed([NotNull] MechanicsContext context, [NotNull] AbilityData ability, [CanBeNull] TargetWrapper target)
		{
			if (!AbilityFilter.IsPassed(context, ability.Caster, target, null, ability))
			{
				return true;
			}
			if (ForbidAbility)
			{
				return false;
			}
			if (!(target == null))
			{
				return TargetRestriction.IsPassed(context, ability.Caster, target, null, ability);
			}
			return true;
		}
	}

	private readonly List<Entry> _entries = new List<Entry>();

	[Obsolete("WH2-14018")]
	private RestrictionCalculator _interruptionAbilityRestrictions = new RestrictionCalculator();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityRestrictions",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void AddEntry([NotNull] EntityFact fact, [NotNull] BlueprintComponent component, [NotNull] RestrictionCalculator abilityFilter, [NotNull] RestrictionCalculator targetRestriction, bool forbidAbility)
	{
		_entries.Add(new Entry(fact, component, abilityFilter, targetRestriction, forbidAbility));
	}

	public void RemoveEntry([NotNull] EntityFact fact, [NotNull] BlueprintComponent component)
	{
		_entries.RemoveAll((Entry p1) => p1.SourceFact == fact && p1.SourceComponent == component);
		if (_entries.Empty())
		{
			RemoveSelf();
		}
	}

	public bool IsRestrictionPassed([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
	{
		return IsRestrictionPassedInternal(ability, target);
	}

	public bool IsRestrictionPassed([NotNull] AbilityData ability)
	{
		return IsRestrictionPassedInternal(ability, null);
	}

	private bool IsRestrictionPassedInternal([NotNull] AbilityData ability, [CanBeNull] TargetWrapper target)
	{
		if (ability.IgnoreRestrictions)
		{
			return true;
		}
		using AbilityExecutionContext context = ability.ClaimExecutionContext(target ?? ((TargetWrapper)ability.Caster));
		RestrictionCalculator interruptionAbilityRestrictions = _interruptionAbilityRestrictions;
		if (interruptionAbilityRestrictions != null && !interruptionAbilityRestrictions.IsPassed(context, ability.Caster, target, null, ability))
		{
			return false;
		}
		foreach (Entry entry in _entries)
		{
			if (!entry.IsRestrictionPassed(context, ability, target))
			{
				return false;
			}
		}
		return true;
	}

	[Obsolete("WH2-14018")]
	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		_interruptionAbilityRestrictions = interruptionData.RestrictionsOnInterrupt;
	}

	[Obsolete("WH2-14018")]
	void IInterruptTurnEndHandler.HandleUnitEndInterruptTurn()
	{
		_interruptionAbilityRestrictions = null;
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
		PartAbilityRestrictions source = new PartAbilityRestrictions();
		result = Unsafe.As<PartAbilityRestrictions, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityRestrictions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityRestrictions>();
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
