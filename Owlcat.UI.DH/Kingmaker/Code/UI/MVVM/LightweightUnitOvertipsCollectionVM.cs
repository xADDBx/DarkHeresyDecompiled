using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LightweightUnitOvertipsCollectionVM : OvertipsCollectionVM<LightweightUnitOvertipVM>, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IGameModeHandler, IReloadMechanicsHandler, IAreaActivationHandler, IAreaHandler, ISurroundingInteractableObjectsCountHandler, IEntitySuppressedHandler, ISubscriber<IEntity>, IInGameHandler
{
	private static readonly float DeathEntityRemoveDelay = 4f;

	private readonly ReactiveProperty<bool> m_TBMEnabled = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsCutscene = new ReactiveProperty<bool>(value: false);

	private readonly Dictionary<Entity, IDisposable> m_DelayedRemoveHandlers = new Dictionary<Entity, IDisposable>();

	public ReadOnlyReactiveProperty<bool> TBMEnabled => m_TBMEnabled;

	public ReadOnlyReactiveProperty<bool> IsCutscene => m_IsCutscene;

	protected override IEnumerable<Entity> Entities => Game.Instance.EntityPools.AllUnits.All.Where((AbstractUnitEntity e) => e.IsInGame && e is LightweightUnitEntity);

	protected override bool OvertipGetter(LightweightUnitOvertipVM vm, Entity entity)
	{
		if (vm.Unit != entity as MechanicEntity)
		{
			return vm.MechanicEntityUIWrapper.UniqueId == entity.UniqueId;
		}
		return true;
	}

	public LightweightUnitOvertipsCollectionVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
		IsCutscene.Subscribe(delegate(bool value)
		{
			if (value)
			{
				Overtips.ForEach(delegate(LightweightUnitOvertipVM o)
				{
					o.HideBark();
				});
			}
		}).AddTo(this);
		RescanEntities();
	}

	protected override void Clear()
	{
		m_DelayedRemoveHandlers.Values.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_DelayedRemoveHandlers.Clear();
		base.Clear();
	}

	protected override void RemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		base.RemoveEntity(entityData);
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_TBMEnabled.Value = isTurnBased;
		if (!isTurnBased)
		{
			RescanEntities();
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			UnitUIStateHolder.Instance.GetOrCreateUnitState(abstractUnitEntity).UpdateProperties();
			AddEntity(abstractUnitEntity);
		}
	}

	public void HandleEntitySuppressionChanged(IEntity entity, bool suppressed)
	{
		if (entity is LightweightUnitEntity entityData)
		{
			if (suppressed)
			{
				DelayedRemoveEntity(entityData);
			}
			else
			{
				AddEntity(entityData);
			}
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			RemoveEntity(abstractUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			DelayedRemoveEntity(abstractUnitEntity);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsCutscene.Value = gameMode == GameModeType.Cutscene;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		RescanEntities();
	}

	public void OnAreaActivated()
	{
		RescanEntities();
	}

	void IReloadMechanicsHandler.OnMechanicsReloaded()
	{
		RescanEntities();
	}

	void IReloadMechanicsHandler.OnBeforeMechanicsReload()
	{
		Clear();
	}

	public void ShowBark(LightweightUnitEntity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(LightweightUnitEntity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	private void DelayedRemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		float seconds = GetOvertip(entityData)?.DeathDelay ?? DeathEntityRemoveDelay;
		m_DelayedRemoveHandlers[entityData] = DelayedInvoker.InvokeInTime(delegate
		{
			RemoveEntity(entityData);
		}, seconds);
	}

	private void TryCancelDelayedRemoveHandler(Entity entityData)
	{
		if (m_DelayedRemoveHandlers.ContainsKey(entityData))
		{
			m_DelayedRemoveHandlers.Remove(entityData);
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		m_TBMEnabled.Value = true;
	}

	public void HandleSurroundingInteractableObjectsCountChanged(EntityViewBase entity, bool isInNavigation, bool isChosen)
	{
		GetOvertip(entity.Data.ToEntity())?.HandleSurroundingObjectsChanged(isInNavigation, isChosen);
	}

	public void HandleObjectInGameChanged()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			AddEntity(abstractUnitEntity);
		}
	}
}
