using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Footprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Controllers;

public class FootprintsController : IControllerTick, IController, IControllerStop, IUnitFootstepAnimationEventHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	private readonly FootprintPool _pool = new FootprintPool();

	private readonly FootprintLifetimeTracker _tracker = new FootprintLifetimeTracker();

	private readonly FootprintSpawner _spawner;

	private readonly Dictionary<AbstractUnitEntity, UnitFootprintState> _unitStates = new Dictionary<AbstractUnitEntity, UnitFootprintState>();

	private readonly HashSet<AbstractUnitEntity> _pendingUnits = new HashSet<AbstractUnitEntity>();

	public FootprintsController()
	{
		_spawner = new FootprintSpawner(_pool, _tracker);
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		using (ProfileScope.New("Footprints.Tick"))
		{
			float deltaTime = Game.Instance.Controllers.TimeController.DeltaTime;
			FxRoot fxRoot = ConfigRoot.Instance.FxRoot;
			using (ProfileScope.New("Footprints.LifetimeTick"))
			{
				_tracker.Tick(deltaTime, fxRoot, _unitStates, _pool);
			}
			using (ProfileScope.New("Footprints.Spawn"))
			{
				foreach (AbstractUnitEntity pendingUnit in _pendingUnits)
				{
					try
					{
						if (_unitStates.TryGetValue(pendingUnit, out var value))
						{
							_spawner.TrySpawn(pendingUnit, value, fxRoot);
							value.PendingFoot = null;
						}
					}
					catch (Exception ex)
					{
						PFLog.Default.Exception(ex);
					}
				}
				_pendingUnits.Clear();
			}
		}
	}

	public void OnStop()
	{
		_tracker.ReleaseAll(_pool);
		_pool.Clear();
		_unitStates.Clear();
		_pendingUnits.Clear();
	}

	public void HandleUnitFootstepAnimationEvent(string locator, int footIndex)
	{
		using (ProfileScope.New("Footprints.AnimEvent"))
		{
			AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
			if (abstractUnitEntity == null || !abstractUnitEntity.IsVisibleForPlayer)
			{
				return;
			}
			FootprintsMode value = SettingsRoot.Graphics.FootprintsMode.GetValue();
			if (value == FootprintsMode.Off)
			{
				return;
			}
			if (locator == string.Empty)
			{
				PFLog.TechArt.Error("FootprintsController: locator feeld in animation event PlaceFootstep is empty! Check that all fields are filled in animation event! " + abstractUnitEntity.View.name + " animation: " + abstractUnitEntity.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name);
				return;
			}
			UnitPartCompanion optional = abstractUnitEntity.GetOptional<UnitPartCompanion>();
			bool flag = optional != null && optional.State == CompanionState.InParty;
			if (value == FootprintsMode.Party && !flag)
			{
				return;
			}
			FootLocator? footLocator = ResolveLocator(abstractUnitEntity, locator);
			if (footLocator.HasValue)
			{
				if (!_unitStates.TryGetValue(abstractUnitEntity, out var value2))
				{
					int capacity = (flag ? ConfigRoot.Instance.FxRoot.MaxFootprintsCountPerPlayerUnit : Mathf.RoundToInt((float)ConfigRoot.Instance.FxRoot.MaxFootprintsCountPerPlayerUnit * ConfigRoot.Instance.FxRoot.MaxFootprintsCountModForNPC));
					value2 = new UnitFootprintState
					{
						InParty = flag,
						ActiveFootprints = new List<Footprint>
						{
							Capacity = capacity
						}
					};
					_unitStates[abstractUnitEntity] = value2;
				}
				value2.PendingFoot = footLocator.Value;
				value2.PendingFootIndex = footIndex;
				_pendingUnits.Add(abstractUnitEntity);
			}
		}
	}

	[CanBeNull]
	private static FootLocator? ResolveLocator(AbstractUnitEntity unit, string locator)
	{
		Transform transform = unit.View.ParticlesSnapMap.Bones.FirstOrDefault((FxBone b) => b.Name == locator)?.Transform;
		if (transform == null)
		{
			PFLog.TechArt.Error("FootprintsController: " + locator + " not found in " + unit.View.name + " for animation: " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name);
			return null;
		}
		bool leftSided = locator.Contains("left", StringComparison.InvariantCultureIgnoreCase);
		return new FootLocator(transform, leftSided);
	}
}
