using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PointMarkersVM : ViewModel, ILineOfSightHandler, ISubscriber, INetAddPingMarker, IAreaHandler, IAreaActivationHandler, IReloadMechanicsHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ICompanionStateChanged, ISubscriber<IMechanicEntity>
{
	public readonly ObservableList<PointMarkerVM> PointMarkers = new ObservableList<PointMarkerVM>();

	private readonly List<BaseUnitEntity> m_Units = new List<BaseUnitEntity>();

	private Coroutine m_DirtyUnitsCoroutine;

	public IEnumerable<PointMarkerVM> VisibleMarkers => PointMarkers.Where((PointMarkerVM vm) => vm.IsVisible.CurrentValue);

	private IEnumerable<BaseUnitEntity> UnitsSelector => Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity unit) => (unit.Faction.IsPlayer && unit.IsDirectlyControllable) || (unit.IsInCombat && unit.Faction.IsPlayerEnemy));

	public PointMarkersVM()
	{
		SetEntities();
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			UpdateHandler();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
	}

	protected override void OnDispose()
	{
		if (m_DirtyUnitsCoroutine != null)
		{
			CoroutineRunner.Stop(m_DirtyUnitsCoroutine);
		}
		Clear();
	}

	private void Clear()
	{
		PointMarkers.ForEach(delegate(PointMarkerVM markerVm)
		{
			markerVm.Dispose();
		});
		PointMarkers.Clear();
		m_Units.Clear();
	}

	private void UpdateHandler()
	{
		PointMarkers.ForEach(delegate(PointMarkerVM markerVm)
		{
			markerVm.Update();
		});
	}

	private void SetEntities()
	{
		Clear();
		foreach (BaseUnitEntity item in UnitsSelector)
		{
			PointMarkers.Add(new PointMarkerVM(item));
			m_Units.Add(item);
		}
	}

	private IEnumerator DirtyUnits()
	{
		yield return null;
		yield return new WaitUntil(() => !ResourcesLibrary.Preloading);
		while (LoadingProcess.Instance.IsLoadingInProcess)
		{
			yield return null;
		}
		SetEntities();
		m_DirtyUnitsCoroutine = null;
	}

	private async Task UpdateUnits()
	{
		await Awaiters.UnityThread;
		if (m_DirtyUnitsCoroutine != null)
		{
			CoroutineRunner.Stop(m_DirtyUnitsCoroutine);
		}
		m_DirtyUnitsCoroutine = CoroutineRunner.Start(DirtyUnits());
	}

	public void OnLineOfSightCreated(LineOfSightVM los)
	{
		PointMarkers.FirstOrDefault((PointMarkerVM m) => m?.Unit == los?.Owner)?.SetLineOfSightVM(los);
	}

	public void OnLineOfSightDestroyed(LineOfSightVM los)
	{
		PointMarkers.FirstOrDefault((PointMarkerVM m) => m?.Unit == los?.Owner)?.SetLineOfSightVM(null);
	}

	private void AddPingEntity(Entity entity, GameObject pingPosition)
	{
		if (pingPosition != null)
		{
			PointMarkers.Add(new PointMarkerVM(pingPosition));
		}
		else
		{
			PointMarkers.Add(new PointMarkerVM(entity, isPing: true));
		}
	}

	private void RemovePingEntity(Entity entity, GameObject pingPosition)
	{
		if (pingPosition != null)
		{
			PointMarkerVM pointMarkerVM = PointMarkers.FirstOrDefault((PointMarkerVM m) => m.PingPosition == pingPosition);
			pointMarkerVM?.Dispose();
			PointMarkers.Remove(pointMarkerVM);
		}
		else
		{
			PointMarkerVM pointMarkerVM2 = PointMarkers.FirstOrDefault((PointMarkerVM m) => m.AnotherEntity == entity);
			pointMarkerVM2?.Dispose();
			PointMarkers.Remove(pointMarkerVM2);
		}
	}

	public void HandleAddPingEntityMarker(Entity entity)
	{
		AddPingEntity(entity, null);
	}

	public void HandleRemovePingEntityMarker(Entity entity)
	{
		RemovePingEntity(entity, null);
	}

	public void HandleAddPingPositionMarker(GameObject gameObject)
	{
		AddPingEntity(null, gameObject);
	}

	public void HandleRemovePingPositionMarker(GameObject gameObject)
	{
		RemovePingEntity(null, gameObject);
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		UpdateUnits();
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && m_Units.Contains(baseUnitEntity))
		{
			UpdateUnits();
		}
	}

	void IUnitHandler.HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && m_Units.Contains(baseUnitEntity))
		{
			UpdateUnits();
		}
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		Clear();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		UpdateUnits();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		UpdateUnits();
	}

	void IReloadMechanicsHandler.OnBeforeMechanicsReload()
	{
		Clear();
	}

	void IReloadMechanicsHandler.OnMechanicsReloaded()
	{
		UpdateUnits();
	}

	void ICompanionStateChanged.HandleCompanionStateChanged()
	{
		UpdateUnits();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateUnits();
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateUnits();
	}
}
