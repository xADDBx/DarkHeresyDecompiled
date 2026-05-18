using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Code.UI.MVVM.CombatText;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Squads;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMechanicEntityVM : ViewModel, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler
{
	private readonly PartSquad m_SquadOptional;

	private readonly MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsUnableToAct = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_WillNotTakeTurn = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasControlLossEffects = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_Initiative = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<UnitHealthPartVM> m_UnitHealthPartVM = new ReactiveProperty<UnitHealthPartVM>();

	private readonly ReactiveProperty<ActionPointsVM> m_ActionPointVM = new ReactiveProperty<ActionPointsVM>();

	private readonly ReactiveProperty<bool> m_CanBeShowed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_SquadCount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsInSquad = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsSquadLeader = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<UnitMoraleVM> m_OvertipMoraleVM = new ReactiveProperty<UnitMoraleVM>();

	private readonly ReactiveProperty<SurfaceCombatActionVM> m_ConcentrationVM = new ReactiveProperty<SurfaceCombatActionVM>();

	private ChannelingLogic.InitiativeHolder m_InitiativeHolder;

	protected readonly ReactiveProperty<bool> m_IsCurrent = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_NeedToShow = new ReactiveProperty<bool>();

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly UnitBuffBlockVM UnitBuffs;

	public readonly CombatTextBlockVM CombatTextBlockVM;

	public readonly bool IsInitiativeHolder;

	public readonly bool UsedSubtypeIcon;

	public readonly ReadOnlyReactiveProperty<bool> ForceHidePortrait;

	private string InitiativeHolderName => m_UnitUIWrapper.Name + ": " + m_InitiativeHolder.Name;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public ReadOnlyReactiveProperty<bool> IsCurrent => m_IsCurrent;

	public ReadOnlyReactiveProperty<UnitHealthPartVM> UnitHealthPartVM => m_UnitHealthPartVM;

	public ReadOnlyReactiveProperty<ActionPointsVM> ActionPointVM => m_ActionPointVM;

	public ReadOnlyReactiveProperty<bool> IsInSquad => m_IsInSquad;

	public ReadOnlyReactiveProperty<bool> IsSquadLeader => m_IsSquadLeader;

	public ReadOnlyReactiveProperty<bool> NeedToShow => m_NeedToShow;

	public ReadOnlyReactiveProperty<UnitMoraleVM> OvertipMoraleVM => m_OvertipMoraleVM;

	public ReadOnlyReactiveProperty<SurfaceCombatActionVM> ConcentrationVM => m_ConcentrationVM;

	public ReadOnlyReactiveProperty<(bool isEnemy, bool isPlayerFaction)> FactionInfo { get; }

	public UnitSquad Squad { get; }

	public MechanicEntity MechanicEntity => m_UnitUIWrapper.MechanicEntity;

	public BaseUnitEntity UnitAsBaseUnitEntity => MechanicEntity as BaseUnitEntity;

	public bool HasUnit => MechanicEntity != null;

	public string DisplayName
	{
		get
		{
			if (!IsInitiativeHolder)
			{
				return m_UnitUIWrapper.Name;
			}
			return InitiativeHolderName;
		}
	}

	protected CombatMechanicEntityVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	public CombatMechanicEntityVM(MechanicEntity mechanicEntity, ReadOnlyReactiveProperty<bool> forceHidePortrait, bool isCurrent = false)
		: this()
	{
		ForceHidePortrait = forceHidePortrait ?? new ReactiveProperty<bool>(value: false).AddTo(this);
		m_IsCurrent.Value = isCurrent;
		MechanicEntity mechanicEntity2 = mechanicEntity;
		if (mechanicEntity is UnitSquad unitSquad)
		{
			mechanicEntity2 = unitSquad.Leader ?? unitSquad.Units.FirstItem().ToBaseUnitEntity();
			Squad = unitSquad;
		}
		else if (mechanicEntity is ChannelingLogic.InitiativeHolder initiativeHolder)
		{
			m_InitiativeHolder = initiativeHolder;
			mechanicEntity2 = initiativeHolder.Unit;
			IsInitiativeHolder = true;
		}
		else if (mechanicEntity is IInitiativeDelegate { Delegate: { } @delegate })
		{
			mechanicEntity2 = @delegate;
		}
		m_UnitUIWrapper = new MechanicEntityUIWrapper(mechanicEntity2);
		UsedSubtypeIcon = UIUtilityUnit.UsedSubtypeIcon(mechanicEntity2);
		m_SquadOptional = m_UnitUIWrapper.MechanicEntity?.GetSquadOptional();
		m_IsInSquad.Value = m_UnitUIWrapper.IsInSquad;
		m_IsSquadLeader.Value = CheckIsSquadLeader();
		if (Squad == null)
		{
			Squad = m_SquadOptional?.Squad;
		}
		MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(m_UnitUIWrapper.MechanicEntity);
		if (MechanicEntityUIState == null)
		{
			return;
		}
		FactionInfo = MechanicEntityUIState.IsEnemy.CombineLatest(MechanicEntityUIState.IsPlayerFaction, (bool isEnemy, bool isPlayerFaction) => (isEnemy: isEnemy, isPlayerFaction: isPlayerFaction)).ToReadOnlyReactiveProperty().AddTo(this);
		UpdateData();
		UnitBuffs = new UnitBuffBlockVM(UnitAsBaseUnitEntity).AddTo(this);
		m_UnitHealthPartVM.Value = new UnitHealthPartVM(UnitAsBaseUnitEntity).AddTo(this);
		m_OvertipMoraleVM.Value = new UnitMoraleVM(MechanicEntityUIState).AddTo(this);
		UnitBuffs.SetUnitData(m_UnitUIWrapper.MechanicEntity);
		CombatTextBlockVM = new CombatTextBlockVM(MechanicEntityUIState).AddTo(this);
		MechanicEntityUIState.ConcentrationBuff.CombineLatest(MechanicEntityUIState.Channeling, (Buff buff, IUIChanneling channeling) => new { buff, channeling }).Subscribe(value =>
		{
			bool flag = !IsInitiativeHolder && value.channeling != null;
			m_ConcentrationVM.Value = new SurfaceCombatActionVM(flag ? null : value.buff).AddTo(this);
		}).AddTo(this);
		IsCurrent.Subscribe(delegate(bool current)
		{
			if (current)
			{
				m_ActionPointVM.Value = new ActionPointsVM(m_UnitUIWrapper.MechanicEntity).AddTo(this);
				if (UnitAsBaseUnitEntity != null && MechanicEntity.IsPlayerFaction && MechanicEntity.IsViewActive && Game.Instance.CurrentModeType != GameModeType.Cutscene)
				{
					Game.Instance.Controllers.SelectionCharacter.SetSelected(UnitAsBaseUnitEntity);
				}
			}
			else
			{
				ActionPointVM.CurrentValue?.Dispose();
				m_ActionPointVM.Value = null;
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateHandler();
		}).AddTo(this);
		UpdateCanBeShown();
	}

	public void HandleUnitClick()
	{
		ClickUnitHandler.HandleUnitClickWithSelectedAbility(m_UnitUIWrapper.MechanicEntity);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateIsCurrentUnit();
	}

	public void UpdateData()
	{
		if (m_UnitUIWrapper.MechanicEntity != null)
		{
			m_IsPlayer.Value = m_UnitUIWrapper.IsPlayerFaction;
			m_Initiative.Value = (int)m_UnitUIWrapper.Initiative.Roll;
			if ((bool)m_SquadOptional && m_SquadOptional.Squad != null)
			{
				m_IsSquadLeader.Value = CheckIsSquadLeader();
				m_SquadCount.Value = m_SquadOptional.Squad.AliveUnitsCount;
			}
			UnitBuffs?.UpdateData();
			UpdateCanActStates();
			UpdateIsCurrentUnit();
		}
	}

	public void SetMouseHighlighted(bool value)
	{
		if (NeedToShow.CurrentValue)
		{
			UnitAsBaseUnitEntity.View.MouseHoverHighlighting = value;
			return;
		}
		if (Squad != null)
		{
			foreach (UnitReference unit in Squad.Units)
			{
				BaseUnitEntity baseUnitEntity = unit.ToBaseUnitEntity();
				if (MechanicEntity != baseUnitEntity)
				{
					baseUnitEntity.View.SecondaryHighlighting = value;
				}
			}
		}
		UnitAsBaseUnitEntity.View.MouseHoverHighlighting = value;
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateIsCurrentUnit();
	}

	public bool IsCreatedFrom(MechanicEntity mechanicEntity)
	{
		if (IsInitiativeHolder)
		{
			if (mechanicEntity is ChannelingLogic.InitiativeHolder initiativeHolder)
			{
				return initiativeHolder.Unit == MechanicEntity;
			}
			return false;
		}
		return mechanicEntity == MechanicEntity;
	}

	protected override void OnDispose()
	{
	}

	private void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			UpdateCanBeShown();
		}
	}

	private void UpdateIsCurrentUnit()
	{
		if (!Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			m_IsCurrent.Value = Game.Instance.Controllers.TurnController.CurrentUnit == MechanicEntity;
		}
	}

	private void UpdateCanBeShown()
	{
		if (RootUIContext.Instance.FullScreenUIType != 0)
		{
			m_CanBeShowed.Value = false;
		}
		else
		{
			m_CanBeShowed.Value = Game.Instance.CurrentModeType == GameModeType.Default || Game.Instance.CurrentModeType == GameModeType.None || Game.Instance.CurrentModeType == GameModeType.Pause || Game.Instance.CurrentModeType == GameModeType.BugReport || Game.Instance.CurrentModeType == GameModeType.GlobalMap;
		}
	}

	private void UpdateCanActStates()
	{
		if (MechanicEntity is BaseUnitEntity baseUnitEntity)
		{
			m_IsUnableToAct.Value = !baseUnitEntity.CanAct;
			m_WillNotTakeTurn.Value = !WillTakeTurn(baseUnitEntity);
			m_HasControlLossEffects.Value = baseUnitEntity.HasControlLossEffects();
		}
	}

	private bool CheckIsSquadLeader()
	{
		PartSquad squadOptional = m_SquadOptional;
		if (squadOptional == null || !squadOptional.IsInSquad)
		{
			return false;
		}
		MechanicEntityUIWrapper unitUIWrapper = m_UnitUIWrapper;
		if (unitUIWrapper.IsSquadLeader && !unitUIWrapper.IsDeadOrUnconscious)
		{
			return true;
		}
		BaseUnitEntity leader = m_SquadOptional.Leader;
		if (leader != null && !leader.IsDeadOrUnconscious)
		{
			return false;
		}
		UnitReference unitReference = m_SquadOptional.Squad.Units.FirstOrDefault((UnitReference x) => !x.Entity.IsDeadOrUnconscious);
		if (unitReference.Entity != null)
		{
			return unitReference.Entity == m_UnitUIWrapper.MechanicEntity;
		}
		return false;
	}

	private static bool WillTakeTurn(BaseUnitEntity unit)
	{
		if ((bool)unit.Features.Stunned)
		{
			return false;
		}
		if (unit.IsHelpless)
		{
			return false;
		}
		if (unit.IsProne)
		{
			return false;
		}
		return true;
	}
}
