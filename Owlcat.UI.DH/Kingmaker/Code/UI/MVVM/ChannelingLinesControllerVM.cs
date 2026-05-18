using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLinesControllerVM : ViewModel, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandEndHandler, IUnitCommandStartHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaHandler, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, IEventTag<IUnitCommandActHandler, EntitySubscriber>, IHologramPositionChangedHandler, IHologramClearHandler
{
	private bool m_IsDirty = true;

	private readonly ObservableList<ChannelingLineVM> m_LinesVMs = new ObservableList<ChannelingLineVM>();

	private readonly ObservableList<ChannelingLineVM> m_HologramLinesVMs = new ObservableList<ChannelingLineVM>();

	public IEnumerable<ChannelingLineVM> LinesVMs => m_LinesVMs;

	public IEnumerable<ChannelingLineVM> HologramLinesVMs => m_HologramLinesVMs;

	public ChannelingLinesControllerVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		Observable.EveryUpdate().Subscribe(Update).AddTo(this);
	}

	public Observable<CollectionAddEvent<ChannelingLineVM>> ObserveLineAdded()
	{
		return m_LinesVMs.ObserveAdd();
	}

	public Observable<CollectionRemoveEvent<ChannelingLineVM>> ObserveLineRemoved()
	{
		return m_LinesVMs.ObserveRemove();
	}

	public Observable<CollectionAddEvent<ChannelingLineVM>> ObserveHologramLineAdded()
	{
		return m_HologramLinesVMs.ObserveAdd();
	}

	public Observable<CollectionRemoveEvent<ChannelingLineVM>> ObserveHologramLineRemoved()
	{
		return m_HologramLinesVMs.ObserveRemove();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		if ((executor == null || executor.IsInCombat) && TurnController.IsInTurnBasedCombat())
		{
			SetDirty(isDirty: true);
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		if ((executor == null || executor.IsInCombat) && TurnController.IsInTurnBasedCombat())
		{
			SetDirty(isDirty: true);
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.LifeState.State == UnitLifeState.Dead)
		{
			MechanicEntityUIState orCreateUnitState = UnitUIStateHolder.Instance.GetOrCreateUnitState(baseUnitEntity);
			if (orCreateUnitState != null)
			{
				TryClearLine(orCreateUnitState);
			}
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		SetDirty(isDirty: true);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Clear();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			SetDirty(isDirty: true);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		SetDirty(isDirty: true);
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		SetDirty(isDirty: true);
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		SetDirty(isDirty: true);
	}

	public void HandleHologramPositionChanged()
	{
		SyncHologramLines();
	}

	public void HandleHologramClear()
	{
		ClearHologramLines();
	}

	protected override void OnDispose()
	{
		Clear();
	}

	private void Update()
	{
		if (m_IsDirty)
		{
			SetupUnits();
			SetDirty(isDirty: false);
		}
	}

	private void SetDirty(bool isDirty)
	{
		m_IsDirty = isDirty;
	}

	private void Clear()
	{
		m_LinesVMs.ForEach(delegate(ChannelingLineVM l)
		{
			l.Dispose();
		});
		m_LinesVMs.Clear();
		ClearHologramLines();
	}

	private void SetupUnits()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			SetUnitList(Game.Instance.Controllers.TurnController.CurrentRoundUnitsOrder);
			SetUnitList(Game.Instance.Controllers.TurnController.NextRoundUnitsOrder);
			SyncHologramLines();
		}
	}

	private void SetUnitList(IEnumerable<MechanicEntity> units)
	{
		foreach (MechanicEntity unit in units)
		{
			if (!IsUnitValid(unit))
			{
				continue;
			}
			MechanicEntityUIState unitState = UnitUIStateHolder.Instance.GetOrCreateUnitState(unit);
			if (unitState == null)
			{
				continue;
			}
			if (unitState.Channeling.CurrentValue == null)
			{
				TryClearLine(unitState);
			}
			else if (!m_LinesVMs.Contains((ChannelingLineVM l) => l.UnitState == unitState))
			{
				m_LinesVMs.Add(new ChannelingLineVM(unitState, delegate
				{
					TryClearLine(unitState);
				}).AddTo(this));
			}
		}
		static bool IsUnitValid(MechanicEntity unit)
		{
			if (!unit.IsDisposed && unit.IsInGame)
			{
				return !unit.IsInFogOfWar;
			}
			return false;
		}
	}

	private bool TryClearLine(MechanicEntityUIState unitState)
	{
		ChannelingLineVM channelingLineVM = m_LinesVMs.FirstOrDefault((ChannelingLineVM vm) => vm.UnitState == unitState);
		if (channelingLineVM == null)
		{
			return false;
		}
		TryClearHologramLine(unitState);
		return m_LinesVMs.Remove(channelingLineVM);
	}

	private void SyncHologramLines()
	{
		BaseUnitEntity baseUnitEntity = UnitPredictionManager.Instance?.GetHologram()?.Parent;
		if (baseUnitEntity == null)
		{
			ClearHologramLines();
			return;
		}
		foreach (ChannelingLineVM main in m_LinesVMs)
		{
			if (!main.IsHologramLine && main.UnitState.Channeling.CurrentValue?.Target?.Entity == baseUnitEntity && !m_HologramLinesVMs.Any((ChannelingLineVM h) => h.UnitState == main.UnitState))
			{
				MechanicEntityUIState unitState = main.UnitState;
				ChannelingLineVM item = new ChannelingLineVM(unitState, GetHologramPosition, delegate
				{
					TryClearHologramLine(unitState);
				}).AddTo(this);
				m_HologramLinesVMs.Add(item);
			}
		}
		List<ChannelingLineVM> list = null;
		foreach (ChannelingLineVM holo in m_HologramLinesVMs)
		{
			ChannelingLineVM channelingLineVM = m_LinesVMs.FirstOrDefault((ChannelingLineVM l) => l.UnitState == holo.UnitState);
			if (channelingLineVM == null || channelingLineVM.UnitState.Channeling.CurrentValue?.Target?.Entity != baseUnitEntity)
			{
				if (list == null)
				{
					list = new List<ChannelingLineVM>();
				}
				list.Add(holo);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (ChannelingLineVM item2 in list)
		{
			m_HologramLinesVMs.Remove(item2);
		}
	}

	private void ClearHologramLines()
	{
		m_HologramLinesVMs.ForEach(delegate(ChannelingLineVM h)
		{
			h.Dispose();
		});
		m_HologramLinesVMs.Clear();
	}

	private bool TryClearHologramLine(MechanicEntityUIState unitState)
	{
		ChannelingLineVM channelingLineVM = m_HologramLinesVMs.FirstOrDefault((ChannelingLineVM vm) => vm.UnitState == unitState);
		if (channelingLineVM == null)
		{
			return false;
		}
		return m_HologramLinesVMs.Remove(channelingLineVM);
	}

	private static Vector3? GetHologramPosition()
	{
		return UnitPredictionManager.RealHologramPosition;
	}
}
