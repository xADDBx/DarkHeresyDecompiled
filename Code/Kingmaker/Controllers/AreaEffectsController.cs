using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using Pathfinding;

namespace Kingmaker.Controllers;

public class AreaEffectsController : IControllerTick, IController, ITeleportHandler, ISubscriber, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnBasedModeHandler
{
	public readonly struct AreaEffectSpawner
	{
		private readonly BlueprintAreaEffect _blueprint;

		private readonly MechanicsContext _parentContext;

		private readonly TargetWrapper _target;

		private readonly TimeSpan? _duration;

		private readonly bool _onUnit;

		private readonly bool _usePatternFromAbility;

		private readonly bool _getOrientationFromCaster;

		private readonly float? _initiative;

		public AreaEffectSpawner(BlueprintAreaEffect blueprint, MechanicsContext parentContext, TargetWrapper target)
			: this(blueprint, parentContext, target, null, onUnit: false, usePatternFromAbility: false, getOrientationFromCaster: false, null)
		{
		}

		private AreaEffectSpawner(BlueprintAreaEffect blueprint, MechanicsContext parentContext, TargetWrapper target, TimeSpan? duration = null, bool onUnit = false, bool usePatternFromAbility = false, bool getOrientationFromCaster = false, float? initiative = null)
		{
			_blueprint = blueprint;
			_parentContext = parentContext;
			_target = target;
			_duration = duration;
			_onUnit = onUnit;
			_usePatternFromAbility = usePatternFromAbility;
			_getOrientationFromCaster = getOrientationFromCaster;
			_initiative = initiative;
		}

		private AreaEffectSpawner(AreaEffectSpawner spawner, TimeSpan? duration = null, bool? onUnit = null, bool? usePatternFromAbility = null, bool? getOrientationFromCaster = null, float? initiative = null)
			: this(spawner._blueprint, spawner._parentContext, spawner._target, duration ?? spawner._duration, onUnit ?? spawner._onUnit, usePatternFromAbility ?? spawner._usePatternFromAbility, getOrientationFromCaster ?? spawner._getOrientationFromCaster, initiative ?? spawner._initiative)
		{
		}

		public AreaEffectSpawner Duration(TimeSpan duration)
		{
			return new AreaEffectSpawner(this, duration);
		}

		public AreaEffectSpawner OnUnit(bool onUnit = true)
		{
			AreaEffectSpawner spawner = this;
			bool? onUnit2 = onUnit;
			return new AreaEffectSpawner(spawner, null, onUnit2);
		}

		public AreaEffectSpawner UsePatternFromAbility(bool usePatternFromAbility = true)
		{
			AreaEffectSpawner spawner = this;
			bool? usePatternFromAbility2 = usePatternFromAbility;
			return new AreaEffectSpawner(spawner, null, null, usePatternFromAbility2);
		}

		public AreaEffectSpawner GetOrientationFromCaster(bool getOrientationFromCaster = true)
		{
			AreaEffectSpawner spawner = this;
			bool? getOrientationFromCaster2 = getOrientationFromCaster;
			return new AreaEffectSpawner(spawner, null, null, null, getOrientationFromCaster2);
		}

		public AreaEffectSpawner Initiative(float? initiative)
		{
			return new AreaEffectSpawner(this, null, null, null, null, initiative);
		}

		public AreaEffectEntity Spawn()
		{
			if (_onUnit && !(_target.Entity is BaseUnitEntity))
			{
				throw new InvalidOperationException();
			}
			return AreaEffectsController.Spawn(_parentContext, _blueprint, _target, _duration, _onUnit, _usePatternFromAbility, _getOrientationFromCaster, _initiative);
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			areaEffect.Tick();
		}
	}

	private static void TickAreaEffects(bool isTurnBased, Initiative.Event @event)
	{
		if (!isTurnBased && @event != 0)
		{
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Initiative.ShouldActNow(isTurnBased, @event, out var actRound))
			{
				areaEffect.Initiative.LastTurn = actRound;
				areaEffect.NextRound();
			}
		}
	}

	void ITeleportHandler.HandlePartyTeleport(AreaEnterPoint enterPoint)
	{
		Tick();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.TurnStart);
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.RoundStart);
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.RoundEnd);
	}

	private static AreaEffectEntity Spawn(MechanicsContext parentContext, BlueprintAreaEffect blueprint, TargetWrapper target, TimeSpan? duration, bool onUnit, bool usePatternFromAbility, bool getOrientationFromCaster, float? initiative)
	{
		SceneEntitiesState state = ((!onUnit) ? Game.Instance.LoadedAreaState.MainState : (target.Entity?.HoldingState ?? ContextData<EntitySpawnController.EntitySpawnData>.Current?.TargetState));
		AreaEffectEntity areaEffectEntity = Entity.Initialize(new AreaEffectEntity(Uuid.Instance.CreateGuid().ToString(), isInGame: true, parentContext, blueprint, target, Game.Instance.Controllers.TimeController.GameTime, duration, onUnit, usePatternFromAbility, initiative));
		areaEffectEntity.AttachToViewOnLoad(null);
		Game.Instance.Controllers.EntitySpawner.SpawnEntity(areaEffectEntity, state);
		EventBus.RaiseEvent((IAreaEffectEntity)areaEffectEntity, (Action<IAreaEffectHandler>)delegate(IAreaEffectHandler h)
		{
			h.HandleAreaEffectSpawned();
		}, isCheckRuntime: true);
		return areaEffectEntity;
	}

	public static bool CheckConcussionEffect(GridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasConcussionEffect)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckCantAttackEffect(GridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasCantAttackEffect)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckInertWarpEffect(GridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasInertWarpEffect)
			{
				return true;
			}
		}
		return false;
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint.OnlyInCombat)
			{
				areaEffect.ForceEnd();
			}
		}
	}

	public static AreaEffectSpawner CreateSpawner(BlueprintAreaEffect blueprint, MechanicsContext parentContext, TargetWrapper target)
	{
		return new AreaEffectSpawner(blueprint, parentContext, target);
	}
}
