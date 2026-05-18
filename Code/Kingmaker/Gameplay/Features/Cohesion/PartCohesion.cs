using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Cohesion;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartCohesion : UnitPart, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, IActorStatChangedHandler<EntitySubscriber>, IActorStatChangedHandler, ISubscriber<IMechanicEntity>, IEventTag<IActorStatChangedHandler, EntitySubscriber>, IEntitySubscriber, IAreaActivationHandler, IHashable, IOwlPackable<PartCohesion>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<AreaEffectEntity> _areaEffect;

	private Func<MechanicEntity, bool> _enemyPredicate;

	private Func<MechanicEntity, bool> _allyPredicate;

	private Func<MechanicEntity, bool> _anyPredicate;

	private bool _subscribedOnAreaEffect;

	[JsonProperty]
	[OwlPackInclude]
	private CountableFlag _isDangerous = new CountableFlag();

	private AoEPattern _pattern;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartCohesion",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("_areaEffect", typeof(EntityRef<AreaEffectEntity>)),
			new FieldInfo("_isDangerous", typeof(CountableFlag))
		}
	};

	private static BlueprintCombatRoot CombatRoot => ConfigRoot.Instance.CombatRoot;

	[CanBeNull]
	private AreaEffectEntity AreaEffect => _areaEffect.Entity;

	private int RangeValue => base.Owner.Actor.GetStat(StatType.CohesionRange, null, default(StatContext), "RangeValue");

	public int MinRange => CombatRoot.MinCohesionRange;

	public int MaxRange => CombatRoot.MaxCohesionRange;

	public int Range => Math.Clamp(RangeValue, MinRange, MaxRange);

	public IEnumerable<Modifier> RangeModifiers
	{
		get
		{
			StatQueryOutput statQueryOutput = new StatQueryOutput();
			base.Owner.Actor.GetStat(StatType.CohesionRange, statQueryOutput, default(StatContext), "RangeModifiers");
			return statQueryOutput.Modifiers.SortedModifiersList;
		}
	}

	public int EnemiesInRangeCount => CountUnitsInRange(_enemyPredicate);

	public int AlliesInRangeCount => CountUnitsInRange(_allyPredicate);

	public int UnitsInRangeCount => CountUnitsInRange(_anyPredicate);

	[NotNull]
	public AoEPattern Pattern => _pattern ?? (_pattern = AoEPattern.Circle(Range));

	public NodeList? PatternNodes => AreaEffect?.Shape.CoveredNodes;

	public IEnumerable<UnitEntity> UnitsInRange => (from e in AreaEffect?.InGameEntitiesInside.OfType<UnitEntity>()
		where e != base.Owner
		select e) ?? Enumerable.Empty<UnitEntity>();

	private int CountUnitsInRange(Func<MechanicEntity, bool> predicate)
	{
		return AreaEffect?.CountInside(predicate) ?? 0;
	}

	public bool ContainsInRange(MechanicEntity entity)
	{
		if (base.Owner != entity)
		{
			return AreaEffect?.ContainsInside(entity) ?? false;
		}
		return true;
	}

	protected override void OnAttachOrPrePostLoad()
	{
		UnitEntity owner = base.Owner;
		_enemyPredicate = (MechanicEntity e) => owner != e && owner.IsEnemy(e);
		_allyPredicate = (MechanicEntity e) => owner != e && owner.IsAlly(e);
		_anyPredicate = (MechanicEntity e) => owner != e;
	}

	protected override void OnDetach()
	{
		DestroyAreaEffect();
	}

	private void SpawnAreaEffect()
	{
		using (ProfileScope.NewScope("SpawnAreaEffect"))
		{
			AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(CombatRoot.CohesionAreaEffect, base.Owner.MainFact.Context, base.Owner).OnUnit().Spawn();
			areaEffectEntity.OverridePattern(AoEPattern.Circle(Range));
			if ((bool)_isDangerous)
			{
				areaEffectEntity.AddFact(CombatRoot.CohesionDangerousMark);
			}
			_areaEffect = areaEffectEntity;
			SubscribeOnAreaEffect();
		}
	}

	private void DestroyAreaEffect()
	{
		using (ProfileScope.NewScope("DestroyAreaEffect"))
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(_areaEffect.Entity);
			_areaEffect = null;
			_subscribedOnAreaEffect = false;
		}
	}

	private void SubscribeOnAreaEffect()
	{
		AreaEffectEntity areaEffect = AreaEffect;
		if (areaEffect == null)
		{
			throw new InvalidOperationException();
		}
		if (!_subscribedOnAreaEffect)
		{
			areaEffect.OnEntityEnter = (AreaEffectEntity.EntityEventDelegate)Delegate.Combine(areaEffect.OnEntityEnter, new AreaEffectEntity.EntityEventDelegate(OnEntityEnterRange));
			areaEffect.OnEntityExit = (AreaEffectEntity.EntityEventDelegate)Delegate.Combine(areaEffect.OnEntityExit, new AreaEffectEntity.EntityEventDelegate(OnEntityExitRange));
			areaEffect.OnEntityMove = (AreaEffectEntity.EntityEventDelegate)Delegate.Combine(areaEffect.OnEntityMove, new AreaEffectEntity.EntityEventDelegate(OnEntityMoveInRange));
			areaEffect.OnEntityStartTurn = (AreaEffectEntity.EntityEventDelegate)Delegate.Combine(areaEffect.OnEntityStartTurn, new AreaEffectEntity.EntityEventDelegate(OnEntityStartTurnInRange));
			areaEffect.OnEntityEndTurn = (AreaEffectEntity.EntityEventDelegate)Delegate.Combine(areaEffect.OnEntityEndTurn, new AreaEffectEntity.EntityEventDelegate(OnEntityEndTurnInRange));
			_subscribedOnAreaEffect = true;
		}
	}

	private void OnEntityEnterRange(MechanicEntity entity)
	{
		if (entity != base.Owner)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IEntityEnterCohesionRangeHandler>)delegate(IEntityEnterCohesionRangeHandler h)
			{
				h.HandleEntityEnterCohesionRange(entity);
			}, isCheckRuntime: true);
		}
	}

	private void OnEntityExitRange(MechanicEntity entity)
	{
		if (entity != base.Owner)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IEntityExitCohesionRangeHandler>)delegate(IEntityExitCohesionRangeHandler h)
			{
				h.HandleEntityExitCohesionRange(entity);
			}, isCheckRuntime: true);
		}
	}

	private void OnEntityMoveInRange(MechanicEntity entity)
	{
		if (entity != base.Owner)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IEntityMoveInCohesionRangeHandler>)delegate(IEntityMoveInCohesionRangeHandler h)
			{
				h.HandleEntityMoveInCohesionRange(entity);
			}, isCheckRuntime: true);
		}
	}

	private void OnEntityStartTurnInRange(MechanicEntity entity)
	{
		if (entity != base.Owner)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IEntityStartTurnInCohesionRangeHandler>)delegate(IEntityStartTurnInCohesionRangeHandler h)
			{
				h.HandleEntityStartTurnInCohesionRange(entity);
			}, isCheckRuntime: true);
		}
	}

	private void OnEntityEndTurnInRange(MechanicEntity entity)
	{
		if (entity != base.Owner)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IEntityEndTurnInCohesionRangeHandler>)delegate(IEntityEndTurnInCohesionRangeHandler h)
			{
				h.HandleEntityEndTurnInCohesionRange(entity);
			}, isCheckRuntime: true);
		}
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		SpawnAreaEffect();
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		DestroyAreaEffect();
	}

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		if (stats.Contains(StatType.CohesionRange))
		{
			_pattern = null;
			AreaEffect?.OverridePattern(Pattern);
		}
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		if (AreaEffect != null)
		{
			SubscribeOnAreaEffect();
			AreaEffect.OverridePattern(Pattern);
		}
	}

	public void MarkAreaDangerous()
	{
		if (!_isDangerous && AreaEffect != null)
		{
			AreaEffect.AddFact(CombatRoot.CohesionDangerousMark);
		}
		_isDangerous.Retain();
	}

	public void RemoveDangerousMark()
	{
		_isDangerous.Release();
		if (!_isDangerous && AreaEffect != null)
		{
			AreaEffect.Facts.Remove((BlueprintMechanicEntityFact?)CombatRoot.CohesionDangerousMark);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<AreaEffectEntity> obj = _areaEffect;
		Hash128 val2 = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<CountableFlag>.GetHash128(_isDangerous);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartCohesion source = new PartCohesion();
		result = Unsafe.As<PartCohesion, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartCohesion>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_areaEffect", ref _areaEffect, state);
		formatter.Field(1, "_isDangerous", ref _isDangerous, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartCohesion>();
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
				_areaEffect = formatter.ReadPackable<EntityRef<AreaEffectEntity>>(state);
				break;
			case 1:
				_isDangerous = formatter.ReadPackable<CountableFlag>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
