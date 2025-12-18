using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LineOfSightControllerVM : ViewModel, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandEndHandler, IUnitCommandStartHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaHandler, IAreaActivationHandler
{
	public readonly ObservableList<LineOfSightVM> LinesVMs = new ObservableList<LineOfSightVM>();

	private readonly ReactiveCommand<Unit> m_UnitsListIsDirty = new ReactiveCommand<Unit>();

	public Observable<Unit> UnitsListIsDirty => m_UnitsListIsDirty;

	public LineOfSightControllerVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(UnitsListIsDirty.DelayFrame(2, UnityFrameProvider.PostLateUpdate), delegate
		{
			SetNewUnit();
		}).AddTo(this);
		m_UnitsListIsDirty.Execute();
	}

	protected override void OnDispose()
	{
		Clear();
	}

	private void RemoveLine(LineOfSightVM line)
	{
		line.Dispose();
		LinesVMs.Remove(line);
	}

	private LineOfSightVM GetLineByOwner(MechanicEntity owner)
	{
		return LinesVMs.FirstOrDefault((LineOfSightVM vm) => vm.Owner == owner);
	}

	private void Clear()
	{
		LinesVMs.ForEach(delegate(LineOfSightVM l)
		{
			l.Dispose();
		});
		LinesVMs.Clear();
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
			m_UnitsListIsDirty.Execute();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		m_UnitsListIsDirty.Execute();
	}

	private void SetNewUnit()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			Clear();
			MechanicEntity unit = Game.Instance.Controllers.TurnController?.CurrentUnit;
			if (CheckUnit(unit))
			{
				SetUnitList(Game.Instance.Controllers.TurnController?.CurrentRoundUnitsOrder);
				SetUnitList(Game.Instance.Controllers.TurnController?.NextRoundUnitsOrder);
			}
		}
	}

	private void SetUnitList(IEnumerable<MechanicEntity> units)
	{
		MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
		foreach (MechanicEntity unit in units)
		{
			if (unit.IsPlayerEnemy && !unit.IsDisposed)
			{
				LinesVMs.Add(new LineOfSightVM(currentUnit, unit));
			}
		}
	}

	private bool CheckUnit(MechanicEntity unit)
	{
		if (unit != null && !unit.IsPlayerEnemy)
		{
			return !(unit is UnitSquad);
		}
		return false;
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor.IsInCombat && TurnController.IsInTurnBasedCombat())
		{
			m_UnitsListIsDirty.Execute();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor.IsInCombat && TurnController.IsInTurnBasedCombat())
		{
			Clear();
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
			LineOfSightVM lineByOwner = GetLineByOwner(baseUnitEntity);
			if (lineByOwner != null)
			{
				RemoveLine(lineByOwner);
			}
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		m_UnitsListIsDirty.Execute();
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		m_UnitsListIsDirty.Execute();
	}

	public void OnAreaActivated()
	{
		m_UnitsListIsDirty.Execute();
	}
}
