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
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMechanicEntityVM : ViewModel, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IAbilityTargetSelectionUIHandler
{
	private readonly PartSquad m_SquadOptional;

	private readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsNeutral = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsUnableToAct = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_WillNotTakeTurn = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasControlLossEffects = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsTargetSelection = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_Intiative = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<UnitHealthPartVM> m_UnitHealthPartVM = new ReactiveProperty<UnitHealthPartVM>();

	private readonly ReactiveProperty<ActionPointsVM> m_ActionPointVM = new ReactiveProperty<ActionPointsVM>();

	private readonly ReactiveProperty<bool> m_CanBeShowed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_SquadCount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsInSquad = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsSquadLeader = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasAliveUnitsInSquad = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_SquadGroupIndex = new ReactiveProperty<int>(-1);

	private readonly ReactiveProperty<UnitMoraleVM> m_OvertipMoraleVM = new ReactiveProperty<UnitMoraleVM>();

	private readonly ReactiveProperty<SurfaceCombatActionVM> m_ConcentrationVM = new ReactiveProperty<SurfaceCombatActionVM>();

	private ChannelingLogic.InitiativeHolder m_InitiativeHolder;

	protected readonly ReactiveProperty<bool> m_IsCurrent = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_NeedToShow = new ReactiveProperty<bool>();

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly MechanicEntityUIWrapper UnitUIWrapper;

	public readonly UnitBuffBlockVM UnitBuffs;

	public readonly CombatTextBlockVM CombatTextBlockVM;

	public readonly bool IsInitiativeHolder;

	public readonly bool UsedSubtypeIcon;

	private string InitiativeHolderName => UnitUIWrapper.Name + ": " + m_InitiativeHolder.Name;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<bool> IsNeutral => m_IsNeutral;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public ReadOnlyReactiveProperty<bool> IsCurrent => m_IsCurrent;

	public ReadOnlyReactiveProperty<bool> IsTargetSelection => m_IsTargetSelection;

	public ReadOnlyReactiveProperty<UnitHealthPartVM> UnitHealthPartVM => m_UnitHealthPartVM;

	public ReadOnlyReactiveProperty<ActionPointsVM> ActionPointVM => m_ActionPointVM;

	public ReadOnlyReactiveProperty<bool> IsInSquad => m_IsInSquad;

	public ReadOnlyReactiveProperty<bool> IsSquadLeader => m_IsSquadLeader;

	public ReadOnlyReactiveProperty<bool> HasAliveUnitsInSquad => m_HasAliveUnitsInSquad;

	public ReadOnlyReactiveProperty<bool> NeedToShow => m_NeedToShow;

	public ReadOnlyReactiveProperty<int> SquadGroupIndex => m_SquadGroupIndex;

	public ReadOnlyReactiveProperty<UnitMoraleVM> OvertipMoraleVM => m_OvertipMoraleVM;

	public ReadOnlyReactiveProperty<SurfaceCombatActionVM> ConcentrationVM => m_ConcentrationVM;

	public UnitSquad Squad { get; }

	public bool IsNewActor { get; private set; }

	public MechanicEntity MechanicEntity => UnitUIWrapper.MechanicEntity;

	public BaseUnitEntity UnitAsBaseUnitEntity => MechanicEntity as BaseUnitEntity;

	public bool HasUnit => MechanicEntity != null;

	public bool IsPlayerFaction
	{
		get
		{
			if (HasUnit)
			{
				return MechanicEntity.IsPlayerFaction;
			}
			return false;
		}
	}

	public string DisplayName
	{
		get
		{
			if (!IsInitiativeHolder)
			{
				return UnitUIWrapper.Name;
			}
			return InitiativeHolderName;
		}
	}

	public Sprite SmallPortrait => UnitUIWrapper.SmallPortrait;

	protected CombatMechanicEntityVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	public CombatMechanicEntityVM(MechanicEntity mechanicEntity, bool isCurrent = false)
		: this()
	{
		m_IsCurrent.Value = isCurrent;
		IsNewActor = true;
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
		UnitUIWrapper = new MechanicEntityUIWrapper(mechanicEntity2);
		UsedSubtypeIcon = UIUtilityUnit.UsedSubtypeIcon(mechanicEntity2);
		m_SquadOptional = UnitUIWrapper.MechanicEntity?.GetSquadOptional();
		m_IsInSquad.Value = UnitUIWrapper.IsInSquad;
		m_IsSquadLeader.Value = UnitUIWrapper.IsSquadLeader || (m_SquadOptional?.Squad != null && mechanicEntity2 is BaseUnitEntity baseUnitEntity && m_SquadOptional.Squad.Units.FirstItem() == baseUnitEntity);
		if (Squad == null)
		{
			Squad = m_SquadOptional?.Squad;
		}
		if (UnitUIWrapper.IsInSquad && m_SquadOptional != null)
		{
			m_SquadGroupIndex.Value = Game.Instance.Controllers.TurnController.UnitSquads.IndexOf(m_SquadOptional.Squad);
		}
		MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(UnitUIWrapper.MechanicEntity);
		if (MechanicEntityUIState == null)
		{
			return;
		}
		UpdateData();
		UnitBuffs = new UnitBuffBlockVM(UnitAsBaseUnitEntity).AddTo(this);
		m_UnitHealthPartVM.Value = new UnitHealthPartVM(UnitAsBaseUnitEntity).AddTo(this);
		m_OvertipMoraleVM.Value = new UnitMoraleVM(MechanicEntityUIState).AddTo(this);
		UnitBuffs.SetUnitData(UnitUIWrapper.MechanicEntity);
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
				m_ActionPointVM.Value = new ActionPointsVM(UnitUIWrapper.MechanicEntity).AddTo(this);
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

	public void HandleUnitClick(bool isDoubleClick = false)
	{
		ClickUnitHandler.HandleClickControllableUnit(UnitUIWrapper.MechanicEntity, isDoubleClick);
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
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

	private void UpdateIsCurrentUnit()
	{
		if (!Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			m_IsCurrent.Value = Game.Instance.Controllers.TurnController.CurrentUnit == MechanicEntity;
		}
	}

	public void HandleShow()
	{
		m_NeedToShow.Value = !NeedToShow.CurrentValue;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		if (MechanicEntityUIState != null)
		{
			m_IsTargetSelection.Value = true;
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		if (MechanicEntityUIState != null)
		{
			m_IsTargetSelection.Value = false;
		}
	}

	public void UpdateData()
	{
		if (UnitUIWrapper.MechanicEntity == null)
		{
			return;
		}
		m_IsEnemy.Value = UnitUIWrapper.IsPlayerEnemy;
		m_IsNeutral.Value = UnitUIWrapper.IsNeutral;
		m_IsPlayer.Value = UnitUIWrapper.IsPlayerFaction;
		m_Intiative.Value = (int)UnitUIWrapper.Initiative.Roll;
		if ((bool)m_SquadOptional && m_SquadOptional.Squad != null)
		{
			m_IsSquadLeader.Value = UnitUIWrapper.IsSquadLeader || m_SquadOptional.Squad.Units.FirstOrDefault((UnitReference x) => !x.Entity.IsDead).Entity == UnitUIWrapper.MechanicEntity;
			m_HasAliveUnitsInSquad.Value = m_SquadOptional.Squad.Units.Count((UnitReference x) => !x.Entity.IsDead) > 1;
			m_SquadCount.Value = m_SquadOptional.Squad.Units.Count((UnitReference x) => !x.Entity.IsDead);
		}
		UnitBuffs?.UpdateData();
		UpdateCanActStates();
		UpdateIsCurrentUnit();
		IsNewActor = false;
	}

	public void SetMouseHighlighted(bool value)
	{
		if (NeedToShow.CurrentValue)
		{
			UnitAsBaseUnitEntity.View.MouseHoverHighlighting = value;
			return;
		}
		if (Squad == null)
		{
			UnitAsBaseUnitEntity.View.MouseHoverHighlighting = value;
			return;
		}
		foreach (UnitReference unit in Squad.Units)
		{
			unit.ToBaseUnitEntity().View.MouseHoverHighlighting = value;
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

	public void InvokeUnitViewHighlight(bool state)
	{
		if (UnitUIWrapper.MechanicEntity is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.View.HandleHoverChange(state);
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateIsCurrentUnit();
	}

	private void UpdateCanBeShown()
	{
		if (RootUIContext.Instance.FullScreenUIType != 0)
		{
			m_CanBeShowed.Value = false;
		}
		else
		{
			m_CanBeShowed.Value = Game.Instance.CurrentModeType == GameModeType.Default || Game.Instance.CurrentModeType == GameModeType.None || Game.Instance.CurrentModeType == GameModeType.Pause || Game.Instance.CurrentModeType == GameModeType.BugReport || Game.Instance.CurrentModeType == GameModeType.GlobalMap || Game.Instance.CurrentModeType == GameModeType.StarSystem || Game.Instance.CurrentModeType == GameModeType.SpaceCombat;
		}
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
}
