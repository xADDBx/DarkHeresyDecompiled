using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerVM : ViewModel, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IInGameHandler, ISubscriber<IEntity>, IUnitDirectHoverUIHandler, IUnitMountHandler, IInitiativeChangeHandler, IInitiativeTrackerShowGroup, IEntityDestructionHandler, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>
{
	private readonly ReactiveProperty<bool> m_CanBeVisible = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasUnits = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_HoveredEntity = new ReactiveProperty<CombatMechanicEntityVM>();

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentUnit = new ReactiveProperty<CombatMechanicEntityVM>();

	private readonly ReactiveProperty<int> m_RoundCounter = new ReactiveProperty<int>();

	private readonly ReactiveCommand<Unit> m_EntitiesUpdated = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_ConsoleActive = new ReactiveProperty<bool>();

	private List<InitiativeTrackerMechanicEntityVM> m_TrackerEntities = new List<InitiativeTrackerMechanicEntityVM>();

	private bool m_NeedUpdate;

	public ReadOnlyReactiveProperty<bool> CanBeVisible => m_CanBeVisible;

	public ReadOnlyReactiveProperty<bool> HasUnits => m_HasUnits;

	public IReadOnlyList<InitiativeTrackerMechanicEntityVM> TrackerEntities => m_TrackerEntities;

	public ReadOnlyReactiveProperty<CombatMechanicEntityVM> HoveredEntity => m_HoveredEntity;

	public ReadOnlyReactiveProperty<CombatMechanicEntityVM> CurrentUnit => m_CurrentUnit;

	public ReadOnlyReactiveProperty<int> RoundCounter => m_RoundCounter;

	public InitiativeTrackerMechanicEntityVM RoundVM { get; private set; }

	public int RoundIndex { get; private set; }

	public Observable<Unit> EntitiesUpdated => m_EntitiesUpdated;

	public bool SkipScroll { get; set; }

	public ReadOnlyReactiveProperty<bool> ConsoleActive => m_ConsoleActive;

	public MoraleBalanceVM MoraleBalanceVM { get; }

	public CombatUnitCounterVM CombatUnitCounterVM { get; }

	public InitiativeTrackerVM()
	{
		MoraleBalanceVM = new MoraleBalanceVM().AddTo(this);
		CombatUnitCounterVM = new CombatUnitCounterVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		UpdateEntities();
		m_RoundCounter.Value = Game.Instance.Controllers.TurnController.CombatRound;
		RoundCounter.Subscribe(delegate
		{
			CreateRoundVM();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			TryUpdateEntities();
		}).AddTo(this);
		GameUIState.Instance.GameMode.CombineLatest(GameUIState.Instance.CurrentFullScreenUIType, GameUIState.Instance.IsInCombat, (GameModeType _, FullScreenUIType _, bool _) => new { }).Subscribe(_ =>
		{
			UpdateVisibility();
		}).AddTo(this);
		m_NeedUpdate = true;
	}

	public void SetConsoleActive(bool consoleActive)
	{
		m_ConsoleActive.Value = consoleActive;
	}

	protected override void OnDispose()
	{
		m_NeedUpdate = false;
		m_HasUnits.Value = false;
		m_TrackerEntities.ForEach(delegate(InitiativeTrackerMechanicEntityVM u)
		{
			u.Dispose();
		});
		m_TrackerEntities.Clear();
	}

	private void UpdateVisibility()
	{
		bool flag = GameUIState.Instance.CurrentFullScreenUIType.Value == FullScreenUIType.Unknown;
		bool flag2 = flag;
		bool flag3 = GameUIState.Instance.GameMode.Value != GameModeType.Dialog && GameUIState.Instance.GameMode.Value != GameModeType.Cutscene && GameUIState.Instance.IsInCombat.Value;
		m_CanBeVisible.Value = flag2 && flag3;
	}

	private void CreateRoundVM()
	{
		if (RoundVM == null)
		{
			RoundVM = new InitiativeTrackerMechanicEntityVM(RoundCounter.CurrentValue + 1).AddTo(this);
		}
		else
		{
			RoundVM.SetRound(RoundCounter.CurrentValue + 1);
		}
	}

	private void TryUpdateEntities()
	{
		if (m_NeedUpdate)
		{
			UpdateEntities();
		}
	}

	private void UpdateEntities()
	{
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			return;
		}
		TurnController turnController = Game.Instance.Controllers.TurnController;
		MechanicEntity mechanicEntity = turnController.CurrentUnit ?? turnController.TurnOrder.CurrentRoundUnitsOrder.FirstOrDefault();
		if (mechanicEntity == null)
		{
			return;
		}
		using (ProfileScope.New("InitiativeTrackerVM.UpdateEntities"))
		{
			bool isDirty = false;
			int count = m_TrackerEntities.Count;
			int num = 0;
			List<InitiativeTrackerMechanicEntityVM> list = new List<InitiativeTrackerMechanicEntityVM>();
			InitiativeTrackerMechanicEntityVM initiativeTrackerMechanicEntityVM = EnsureTrackerEntity(mechanicEntity, num, isCurrent: true, ref isDirty);
			list.Add(initiativeTrackerMechanicEntityVM);
			m_CurrentUnit.Value = initiativeTrackerMechanicEntityVM;
			IReadOnlyList<UnitReference> readOnlyList = (mechanicEntity as UnitSquad)?.Units;
			foreach (MechanicEntity entity in turnController.CurrentRoundUnitsOrder)
			{
				if (entity != mechanicEntity && (readOnlyList == null || !readOnlyList.Contains((UnitReference e) => e.ToBaseUnitEntity() == entity)) && CheckVisibiltyInTracker(entity))
				{
					initiativeTrackerMechanicEntityVM = EnsureTrackerEntity(entity, ++num, isCurrent: false, ref isDirty);
					list.Add(initiativeTrackerMechanicEntityVM);
				}
			}
			RoundIndex = num;
			foreach (MechanicEntity item in turnController.NextRoundUnitsOrder.Except(turnController.CurrentRoundUnitsOrder))
			{
				if (CheckVisibiltyInTracker(item))
				{
					initiativeTrackerMechanicEntityVM = EnsureTrackerEntity(item, ++num, isCurrent: false, ref isDirty);
					list.Add(initiativeTrackerMechanicEntityVM);
				}
			}
			if (num != count)
			{
				isDirty = true;
			}
			if (isDirty)
			{
				foreach (InitiativeTrackerMechanicEntityVM item2 in m_TrackerEntities.Except(list).ToList())
				{
					m_TrackerEntities.Remove(item2);
					item2.Dispose();
				}
				m_TrackerEntities = m_TrackerEntities.OrderBy((InitiativeTrackerMechanicEntityVM u) => u.OrderIndex.CurrentValue).ToList();
				m_HasUnits.Value = Enumerable.Any(m_TrackerEntities);
				m_EntitiesUpdated.Execute(Unit.Default);
			}
			m_NeedUpdate = false;
		}
	}

	private bool CheckVisibiltyInTracker(MechanicEntity entity)
	{
		if (entity is IInitiativeDelegate { Delegate: { } @delegate })
		{
			entity = @delegate;
		}
		if (entity is UnitSquad)
		{
			return false;
		}
		if (!entity.IsInGame || entity.IsInFogOfWar)
		{
			if (entity is BaseUnitEntity baseUnitEntity)
			{
				return baseUnitEntity.IsSummoned();
			}
			return false;
		}
		return true;
	}

	private InitiativeTrackerMechanicEntityVM EnsureTrackerEntity(MechanicEntity mechanicEntity, int index, bool isCurrent, ref bool isDirty)
	{
		int num = m_TrackerEntities.FindIndex((InitiativeTrackerMechanicEntityVM v) => v.IsCreatedFrom(mechanicEntity));
		InitiativeTrackerMechanicEntityVM initiativeTrackerMechanicEntityVM = ((num >= 0) ? m_TrackerEntities[num] : null);
		if (initiativeTrackerMechanicEntityVM == null)
		{
			InitiativeTrackerMechanicEntityVM initiativeTrackerMechanicEntityVM2 = new InitiativeTrackerMechanicEntityVM(mechanicEntity, index, isCurrent);
			m_TrackerEntities.Add(initiativeTrackerMechanicEntityVM2);
			isDirty = true;
			return initiativeTrackerMechanicEntityVM2;
		}
		if (initiativeTrackerMechanicEntityVM.OrderIndex.CurrentValue != index || initiativeTrackerMechanicEntityVM.IsCurrent.CurrentValue != isCurrent)
		{
			initiativeTrackerMechanicEntityVM.UpdateData(index, isCurrent);
			isDirty = true;
		}
		else
		{
			initiativeTrackerMechanicEntityVM.UpdateData();
		}
		return initiativeTrackerMechanicEntityVM;
	}

	void IUnitDirectHoverUIHandler.HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if (!isHover)
		{
			m_HoveredEntity.Value = null;
			return;
		}
		InitiativeTrackerMechanicEntityVM value = null;
		foreach (InitiativeTrackerMechanicEntityVM trackerEntity in m_TrackerEntities)
		{
			if (trackerEntity.MechanicEntity?.View == unitEntityView)
			{
				value = trackerEntity;
				break;
			}
		}
		m_HoveredEntity.Value = value;
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		m_NeedUpdate = true;
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		m_NeedUpdate = true;
	}

	void IUnitHandler.HandleUnitDeath()
	{
		m_NeedUpdate = true;
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		m_NeedUpdate = true;
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		m_NeedUpdate = true;
	}

	void IInGameHandler.HandleObjectInGameChanged()
	{
		m_NeedUpdate |= EventInvokerExtensions.MechanicEntity is ICombatParticipant;
	}

	void IUnitMountHandler.HandleUnitMount(BaseUnitEntity mount)
	{
		m_NeedUpdate = true;
	}

	void IUnitMountHandler.HandleUnitDismount([CanBeNull] BaseUnitEntity mount)
	{
		m_NeedUpdate = true;
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_NeedUpdate = isTurnBased;
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		m_NeedUpdate = true;
		m_RoundCounter.Value = Game.Instance.Controllers.TurnController.CombatRound;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		m_NeedUpdate = true;
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		m_NeedUpdate = true;
	}

	void IInitiativeChangeHandler.HandleInitiativeChanged()
	{
		m_NeedUpdate = true;
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		m_NeedUpdate = true;
	}

	void IInitiativeTrackerShowGroup.HandleShowChange()
	{
		m_NeedUpdate = true;
		SkipScroll = true;
	}

	void IEntityDestructionHandler.HandleEntityDestroyed()
	{
		m_NeedUpdate |= EventInvokerExtensions.MechanicEntity is ICombatParticipant;
	}

	void IAreaEffectHandler.HandleAreaEffectSpawned()
	{
		m_NeedUpdate = true;
	}

	void IAreaEffectHandler.HandleAreaEffectDestroyed()
	{
		m_NeedUpdate = true;
	}
}
