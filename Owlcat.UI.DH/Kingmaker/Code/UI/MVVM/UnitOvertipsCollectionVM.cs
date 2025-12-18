using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.OBSOLETE;
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

public class UnitOvertipsCollectionVM : OvertipsCollectionVM<OvertipUnitVM>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IGameModeHandler, IReloadMechanicsHandler, IAreaActivationHandler, IAreaHandler, ISurroundingInteractableObjectsCountHandler, IEntitySuppressedHandler, ISubscriber<IEntity>, IInGameHandler
{
	private static readonly float DeathEntityRemoveDelay = 4f;

	private readonly Dictionary<Entity, IDisposable> m_DelayedRemoveHandlers = new Dictionary<Entity, IDisposable>();

	protected override IEnumerable<Entity> Entities => Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity e) => e.IsInGame && !e.Suppressed);

	protected override bool OvertipGetter(OvertipUnitVM vm, Entity entity)
	{
		if (vm.Unit != entity as MechanicEntity)
		{
			return vm.MechanicEntityUIWrapper.UniqueId == entity.UniqueId;
		}
		return true;
	}

	public UnitOvertipsCollectionVM()
	{
		EventBus.Subscribe(this).AddTo(this);
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
		OvertipUnitVM overtip = GetOvertip(entityData);
		if (overtip == null || !overtip.MechanicEntityUIState.HasLoot.CurrentValue)
		{
			base.RemoveEntity(entityData);
		}
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return true;
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			AddEntity(abstractUnitEntity);
		}
	}

	public void HandleEntitySuppressionChanged(IEntity entity, bool suppressed)
	{
		if (entity is AbstractUnitEntity entityData && !(entity is LightweightUnitEntity))
		{
			if (suppressed)
			{
				TryImmediatelyRemoveEntity(entityData);
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
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			RemoveEntity(abstractUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			UnitUIStateHolder.Instance.GetOrCreateUnitState(abstractUnitEntity).UpdateProperties();
			TryDelayedRemoveEntity(abstractUnitEntity);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			Overtips.ForEach(delegate(OvertipUnitVM o)
			{
				o.HideBark();
			});
		}
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

	public void ShowBark(AbstractUnitEntity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(AbstractUnitEntity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	private void TryDelayedRemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		float seconds = GetOvertip(entityData)?.DeathDelay ?? DeathEntityRemoveDelay;
		m_DelayedRemoveHandlers[entityData] = DelayedInvoker.InvokeInTime(delegate
		{
			RemoveEntity(entityData);
		}, seconds);
	}

	private void TryImmediatelyRemoveEntity(Entity entityData)
	{
		RemoveEntity(entityData);
	}

	private void TryCancelDelayedRemoveHandler(Entity entityData)
	{
		if (m_DelayedRemoveHandlers.TryGetValue(entityData, out var _))
		{
			m_DelayedRemoveHandlers.Remove(entityData);
		}
	}

	public void HandleSurroundingInteractableObjectsCountChanged(EntityViewBase entity, bool isInNavigation, bool isChosen)
	{
		GetOvertip(entity.Data.ToEntity())?.HandleSurroundingObjectsChanged(isInNavigation, isChosen);
	}

	public void HandleObjectInGameChanged()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity) && abstractUnitEntity.IsInGame)
		{
			AddEntity(abstractUnitEntity);
		}
	}
}
