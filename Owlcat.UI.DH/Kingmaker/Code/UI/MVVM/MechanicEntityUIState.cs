using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MechanicEntityUIState : BaseDisposable, IUnitDirectHoverUIHandler, ISubscriber, IDestructibleHoverUIHandler<EntitySubscriber>, IDestructibleHoverUIHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<IDestructibleHoverUIHandler, EntitySubscriber>, IUnitNameHandler, ISubscriber<IBaseUnitEntity>, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, IInterruptTurnStartHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, ICellAbilityHandler<EntitySubscriber>, ICellAbilityHandler, IEventTag<ICellAbilityHandler, EntitySubscriber>, IAbilityTargetSelectionUIHandler, IAbilityTargetHoverUIHandler, IPartyCombatHandler, IInteractionHighlightUIHandler, IUnitInfoVisibilityUIHandler, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IUnitFeaturesHandler<EntitySubscriber>, IUnitFeaturesHandler, IEventTag<IUnitFeaturesHandler, EntitySubscriber>, IGameModeHandler, IUnitFactionHandler, IUnitChangeAttackFactionsHandler, IUnitCommandStartHandler<EntitySubscriber>, IUnitCommandStartHandler, IEventTag<IUnitCommandStartHandler, EntitySubscriber>, IUnitCommandEndHandler, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, IEventTag<IUnitCommandActHandler, EntitySubscriber>, INetRoleSetHandler, INetStopPlayingHandler, INetPingEntity, ILootDroppedAsAttachedHandler<EntitySubscriber>, ILootDroppedAsAttachedHandler, IEventTag<ILootDroppedAsAttachedHandler, EntitySubscriber>, IDestructibleEntityHandler, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IMoralePhaseHandler<EntitySubscriber>, IMoralePhaseHandler, IEventTag<IMoralePhaseHandler, EntitySubscriber>, IMoraleValueHandler<EntitySubscriber>, IMoraleValueHandler, IEventTag<IMoraleValueHandler, EntitySubscriber>, IGlobalRulebookHandler<RulePerformMoraleChange>, IRulebookHandler<RulePerformMoraleChange>, IGlobalRulebookSubscriber, IVirtualPositionUIHandler, IUnitCombatHandler
{
	private Tween m_PingTween;

	private bool m_UnitBoneScanned;

	private Transform m_Bone;

	private static readonly AbilityPatternCache m_AbilityPatternCache = new AbilityPatternCache();

	private readonly EntityCoverUIState m_CoverState;

	public readonly MechanicEntityUIWrapper MechanicEntity;

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPlayer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPlayerFaction = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsDeadOrUnconsciousIsDead = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsDead = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsCover = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsDestructible = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsDestructibleNotCover = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsTBM = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPreparationTurn = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsInCombat = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsVisibleForPlayer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsCurrentUnitTurn = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_IsMouseOverUnit = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPingUnit = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_ForceHotKeyPressed = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_UnitInfoVisible = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_NeedConsoleHint = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsTarget = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsPreciseAttack = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<AbilityTargetUIData> m_AbilityTargetUIData = new ReactiveProperty<AbilityTargetUIData>(default(AbilityTargetUIData));

	private readonly ReactiveProperty<AbilityTargetUIData> m_AbilityTargetUIInitialData = new ReactiveProperty<AbilityTargetUIData>(default(AbilityTargetUIData));

	private readonly ReactiveProperty<AbilityTargetUIData> m_AbilityTargetUICompareData = new ReactiveProperty<AbilityTargetUIData>(default(AbilityTargetUIData));

	private readonly ReactiveProperty<bool> m_IsAoETarget = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasHiddenCondition = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<AbilityData> m_Ability = new ReactiveProperty<AbilityData>(null);

	private readonly ReactiveProperty<bool> m_IsCaster = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsActing = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HoverSelfTargetAbility = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasLoot = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasArmor = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<Buff> m_ConcentrationBuff = new ReactiveProperty<Buff>(null);

	private readonly ReactiveProperty<IUIChanneling> m_Channeling = new ReactiveProperty<IUIChanneling>(null);

	private readonly ReactiveProperty<IUIUnitMoraleData> m_Morale = new ReactiveProperty<IUIUnitMoraleData>(null);

	private readonly ReactiveProperty<IUIUnitMoraleData> m_MoralePrediction = new ReactiveProperty<IUIUnitMoraleData>(null);

	private readonly ReactiveProperty<bool> m_IsMoraleLeader = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_MoraleChanged = new ReactiveCommand<Unit>();

	private TurnController TurnController => Game.Instance.Controllers.TurnController;

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<bool> IsPlayer => m_IsPlayer;

	public ReadOnlyReactiveProperty<bool> IsPlayerFaction => m_IsPlayerFaction;

	public ReadOnlyReactiveProperty<bool> IsDeadOrUnconsciousIsDead => m_IsDeadOrUnconsciousIsDead;

	public ReadOnlyReactiveProperty<bool> IsDead => m_IsDead;

	public ReadOnlyReactiveProperty<bool> IsCover => m_IsCover;

	public ReadOnlyReactiveProperty<bool> IsDestructible => m_IsDestructible;

	public ReadOnlyReactiveProperty<bool> IsDestructibleNotCover => m_IsDestructibleNotCover;

	public ReadOnlyReactiveProperty<bool> IsTBM => m_IsTBM;

	public ReadOnlyReactiveProperty<bool> IsPreparationTurn => m_IsPreparationTurn;

	public ReadOnlyReactiveProperty<bool> IsInCombat => m_IsInCombat;

	public ReadOnlyReactiveProperty<bool> IsVisibleForPlayer => m_IsVisibleForPlayer;

	public ReadOnlyReactiveProperty<bool> IsCurrentUnitTurn => m_IsCurrentUnitTurn;

	public ReadOnlyReactiveProperty<bool> IsMouseOverUnit => m_IsMouseOverUnit;

	public ReadOnlyReactiveProperty<bool> IsPingUnit => m_IsPingUnit;

	public ReadOnlyReactiveProperty<bool> ForceHotKeyPressed => m_ForceHotKeyPressed;

	public ReadOnlyReactiveProperty<bool> UnitInfoVisible => m_UnitInfoVisible;

	public ReadOnlyReactiveProperty<bool> NeedConsoleHint => m_NeedConsoleHint;

	public ReadOnlyReactiveProperty<bool> IsTarget => m_IsTarget;

	public ReadOnlyReactiveProperty<bool> IsPreciseAttack => m_IsPreciseAttack;

	public ReadOnlyReactiveProperty<AbilityTargetUIData> AbilityTargetUIData => m_AbilityTargetUIData;

	public ReadOnlyReactiveProperty<AbilityTargetUIData> AbilityTargetUICompareData => m_AbilityTargetUICompareData;

	public ReadOnlyReactiveProperty<LosCalculations.CoverType> CoverType => m_CoverState.CoverType;

	public ReadOnlyReactiveProperty<bool> IsAoETarget => m_IsAoETarget;

	public ReadOnlyReactiveProperty<bool> HasHiddenCondition => m_HasHiddenCondition;

	public ReadOnlyReactiveProperty<AbilityData> Ability => m_Ability;

	public ReadOnlyReactiveProperty<bool> IsCaster => m_IsCaster;

	public ReadOnlyReactiveProperty<bool> IsActing => m_IsActing;

	public ReadOnlyReactiveProperty<bool> HoverSelfTargetAbility => m_HoverSelfTargetAbility;

	public ReadOnlyReactiveProperty<bool> HasLoot => m_HasLoot;

	public ReadOnlyReactiveProperty<bool> HasArmor => m_HasArmor;

	public ReadOnlyReactiveProperty<Buff> ConcentrationBuff => m_ConcentrationBuff;

	public ReadOnlyReactiveProperty<IUIChanneling> Channeling => m_Channeling;

	public ReadOnlyReactiveProperty<IUIUnitMoraleData> Morale => m_Morale;

	public ReadOnlyReactiveProperty<IUIUnitMoraleData> MoralePrediction => m_MoralePrediction;

	public ReadOnlyReactiveProperty<bool> IsMoraleLeader => m_IsMoraleLeader;

	public Observable<Unit> MoraleChanged => m_MoraleChanged;

	public bool IsHiddenBySettings { get; private set; }

	public bool CheckCanBeTargeted
	{
		get
		{
			if ((!IsInCombat.CurrentValue || !IsVisibleForPlayer.CurrentValue || IsDeadOrUnconsciousIsDead.CurrentValue || IsPlayerFaction.CurrentValue || !(Ability.CurrentValue != null) || !CanTarget() || Ability.CurrentValue.IsAoe) && !IsMouseOverUnit.CurrentValue)
			{
				return IsAoETarget.CurrentValue;
			}
			return true;
		}
	}

	public MechanicEntityUIState([NotNull] MechanicEntity unit)
	{
		MechanicEntity = new MechanicEntityUIWrapper(unit);
		AddDisposable(m_CoverState = new EntityCoverUIState(unit));
		m_IsDeadOrUnconsciousIsDead.Value = MechanicEntity.IsDead;
		IsHiddenBySettings = MechanicEntity.MechanicEntity.Blueprint.GetComponent<UnitUISettings>()?.OverrideHideName ?? false;
		m_IsInCombat.Value = MechanicEntity.MechanicEntity.IsInCombat;
		PreciseAttackController preciseAttackController = Game.Instance.Controllers.PreciseAttackController;
		AddDisposable(preciseAttackController.Target.Subscribe(UpdatePreciseAttackTarget));
		UpdateProperties();
		AddDisposable(ObservableSubscribeExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			InternalUpdate();
		}));
		AddDisposable(EventBus.Subscribe(this));
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			if (!base.IsDisposed)
			{
				UpdateHiddenConditions();
			}
		});
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(2), delegate
		{
			if (!base.IsDisposed)
			{
				bool valueOrDefault = (Game.Instance?.Controllers?.TurnController?.TurnBasedModeActive).GetValueOrDefault();
				UpdateTBMEntity(valueOrDefault);
			}
		});
	}

	protected override void DisposeImplementation()
	{
	}

	private void InternalUpdate()
	{
		if (MechanicEntity.MechanicEntity != null)
		{
			UpdateIsVisibleForPlayer();
			UpdateIsSelected();
		}
	}

	public void UpdateProperties()
	{
		m_IsPlayer.Value = MechanicEntity.IsPlayer;
		m_IsPlayerFaction.Value = MechanicEntity.IsPlayerFaction;
		m_IsEnemy.Value = MechanicEntity.IsPlayerEnemy;
		OnUnitNameChanged();
		m_IsCover.Value = MechanicEntity.IsCover;
		m_IsDestructible.Value = MechanicEntity.IsDestructible;
		m_IsDestructibleNotCover.Value = MechanicEntity.IsDestructibleNotCover;
		m_IsDead.Value = MechanicEntity.IsDead;
		m_HasLoot.Value = MechanicEntity.IsDeadAndHasAttachedDroppedLoot || MechanicEntity.IsDeadAndHasLoot;
		ReactiveProperty<bool> hasArmor = m_HasArmor;
		ModifiableValue modifiableValue = MechanicEntity.Armor?.Durability;
		hasArmor.Value = ((modifiableValue != null && (int)modifiableValue != 0) ? 1 : 0) > (false ? 1 : 0);
		m_Morale.Value = MechanicEntity.Morale;
		UpdateGamepadHint();
	}

	private TargetWrapper GetTargetWrapper()
	{
		return Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(MechanicEntity.MechanicEntity.View.gameObject, Game.Instance.Controllers.ClickEventsController.WorldPosition);
	}

	private bool CanTarget()
	{
		TargetWrapper targetWrapper = GetTargetWrapper();
		if (targetWrapper != null && Ability.CurrentValue.CanTargetFromDesiredPosition(targetWrapper))
		{
			return CanAoETarget(Ability.CurrentValue.Blueprint.AoETargets);
		}
		return false;
	}

	private bool CanAoETarget(TargetType targetType)
	{
		switch (targetType)
		{
		case TargetType.Enemy:
			return MechanicEntity.IsPlayerEnemy;
		case TargetType.Ally:
			if (!MechanicEntity.IsPlayer)
			{
				return MechanicEntity.IsPlayerFaction;
			}
			return true;
		case TargetType.Any:
			return true;
		default:
			return false;
		}
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if (MechanicEntity.MechanicEntity is AbstractUnitEntity abstractUnitEntity && unitEntityView.Data == abstractUnitEntity)
		{
			m_IsMouseOverUnit.Value = isHover;
			HandleMeleeAttackHovered(isHover);
			UpdateGamepadHint();
		}
	}

	public void HandleHoverChange(bool isHover)
	{
		m_IsMouseOverUnit.Value = isHover;
	}

	public void OnUnitNameChanged()
	{
		m_Name.Value = MechanicEntity.MechanicEntity.GetUnitNameWithPlayer();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitUpdateEvent();
		m_CoverState.UpdateCoverType(isTurnBased);
		UpdateIsUnitTurn(isTurnBased);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitUpdateEvent();
		m_CoverState.UpdateCoverType();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateTBMEntity(isTurnBased);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		UpdateTBMEntity(isTurnBasedMode: true);
	}

	public void HandleEndPreparationTurn()
	{
		UpdateTBMEntity(isTurnBasedMode: true);
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateTBMEntity(isTurnBasedMode: true);
	}

	private void HandleUnitUpdateEvent()
	{
		if (ContextData<EventInvoker>.Current?.InvokerEntity == MechanicEntity.MechanicEntity)
		{
			UpdateTBMEntity(TurnController?.TurnBasedModeActive ?? false);
		}
	}

	private void UpdateTBMEntity(bool isTurnBasedMode)
	{
		bool flag = ((!IsDestructible.CurrentValue) ? MechanicEntity.IsInCombat : isTurnBasedMode);
		m_IsTBM.Value = isTurnBasedMode;
		m_IsInCombat.Value = flag;
		m_IsPreparationTurn.Value = TurnController?.IsPreparationTurn ?? false;
		UpdateIsUnitTurn(isTurnBasedMode);
		if (flag && MechanicEntity.MechanicEntity is BaseUnitEntity)
		{
			UpdateTMBUnit();
		}
	}

	private void UpdateTMBUnit()
	{
		UpdateConcentration();
		UpdateMorale();
	}

	private void UpdateIsUnitTurn(bool isTurnBasedMode)
	{
		bool flag = ((!IsDestructible.CurrentValue) ? MechanicEntity.IsInCombat : isTurnBasedMode);
		MechanicEntity mechanicEntity = TurnController?.CurrentUnit;
		m_IsCurrentUnitTurn.Value = flag && mechanicEntity != null && MechanicEntity.MechanicEntity == mechanicEntity;
	}

	private void UpdateConcentration()
	{
		m_ConcentrationBuff.Value = MechanicEntity.ConcentrationBuff;
		m_Channeling.Value = MechanicEntity.Channeling;
	}

	private void UpdateGamepadHint()
	{
		ReactiveProperty<bool> needConsoleHint = m_NeedConsoleHint;
		int value;
		if (IsMouseOverUnit.CurrentValue)
		{
			MechanicEntityUIWrapper mechanicEntity = MechanicEntity;
			value = (((!mechanicEntity.IsDirectlyControllable && !mechanicEntity.IsPlayerEnemy && (bool)MechanicEntity.MechanicEntity?.GetOptional<PartUnitInteractions>()) || MechanicEntity.IsDeadAndHasAttachedDroppedLoot || MechanicEntity.IsDeadAndHasLoot) ? 1 : 0);
		}
		else
		{
			value = 0;
		}
		needConsoleHint.Value = (byte)value != 0;
	}

	private void UpdateIsVisibleForPlayer()
	{
		m_IsVisibleForPlayer.Value = MechanicEntity.IsVisibleForPlayer && MechanicEntity.IsInCameraFrustum;
	}

	private void UpdateIsSelected()
	{
		bool flag = TurnController?.IsPreparationTurn ?? false;
		if (TurnController.IsInTurnBasedCombat() && flag)
		{
			if (MechanicEntity.MechanicEntity is BaseUnitEntity unit)
			{
				m_IsCurrentUnitTurn.Value = Game.Instance.Controllers.SelectionCharacter.IsSelected(unit);
			}
			else
			{
				m_IsCurrentUnitTurn.Value = false;
			}
		}
	}

	public void UpdatePreciseAttackBodyPartData(PreciseAttackController.BodyPartUIData bodyPartUIData, bool isCompare)
	{
		AbilityData currentValue = Ability.CurrentValue;
		if (!isCompare)
		{
			m_AbilityTargetUIData.Value = GetPreciseTargetData(currentValue, bodyPartUIData?.BodyPart);
		}
		else
		{
			m_AbilityTargetUICompareData.Value = GetPreciseTargetData(currentValue, bodyPartUIData?.BodyPart);
		}
	}

	private void UpdatePreciseAttackTarget(MechanicEntity mechanicEntity)
	{
		m_IsPreciseAttack.Value = mechanicEntity != null;
		m_IsTarget.Value = MechanicEntity.MechanicEntity == mechanicEntity;
	}

	private bool CanBeAbilityTarget()
	{
		if (IsInCombat.CurrentValue || IsDestructible.CurrentValue)
		{
			return !IsDeadOrUnconsciousIsDead.CurrentValue;
		}
		return false;
	}

	private AbilityTargetUIData GetPreciseTargetData(AbilityData ability, BlueprintBodyPart bodyPart)
	{
		if (!CanBeAbilityTarget() || bodyPart == null)
		{
			return default(AbilityTargetUIData);
		}
		MechanicEntity mechanicEntity = MechanicEntity.MechanicEntity;
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(ability.Caster);
		return AbilityTargetUIDataCache.Instance.GetOrCreate(ability, desiredPosition, mechanicEntity, bodyPart, mechanicEntity, null);
	}

	private AbilityTargetUIData GetAbilityTargetData(AbilityData ability)
	{
		if (!CanBeAbilityTarget())
		{
			return default(AbilityTargetUIData);
		}
		if (ability.IsChainLightning())
		{
			return default(AbilityTargetUIData);
		}
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(ability.Caster);
		PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
		ClickWithSelectedAbilityHandler selectedAbilityHandler = Game.Instance.Controllers.SelectedAbilityHandler;
		TargetWrapper targetWrapper = null;
		if (clickEventsController != null && selectedAbilityHandler != null)
		{
			targetWrapper = selectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, ability, desiredPosition);
			if ((object)targetWrapper == null)
			{
				targetWrapper = clickEventsController.WorldPosition;
			}
		}
		IReadOnlyList<MechanicEntity> targetsInPattern = m_AbilityPatternCache.Get(ability, desiredPosition, targetWrapper);
		return AbilityTargetUIDataCache.Instance.GetOrCreate(ability, desiredPosition, MechanicEntity.MechanicEntity, null, targetWrapper?.Entity, targetsInPattern);
	}

	public void HandleCellAbility(AbilityTargetUIData abilityTarget)
	{
		AbilityData currentValue = Ability.CurrentValue;
		m_AbilityTargetUIData.Value = abilityTarget;
		m_IsTarget.Value = true;
		m_IsAoETarget.Value = IsTarget.CurrentValue && currentValue != null && (currentValue.IsAoe || currentValue.IsBurst || currentValue.IsBurstAttack || currentValue.IsChainLightning());
	}

	public void HandleCellAbilityClear()
	{
		m_AbilityTargetUIData.Value = m_AbilityTargetUIInitialData.CurrentValue;
		m_IsTarget.Value = false;
		m_IsAoETarget.Value = false;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_IsAoETarget.Value = IsTarget.CurrentValue && (ability.IsAoe || ability.IsBurst || ability.IsBurstAttack || ability.IsChainLightning());
		m_Ability.Value = ability;
		ReactiveProperty<AbilityTargetUIData> abilityTargetUIData = m_AbilityTargetUIData;
		AbilityTargetUIData value = (m_AbilityTargetUIInitialData.Value = GetAbilityTargetData(ability));
		abilityTargetUIData.Value = value;
		m_IsCaster.Value = ability.Caster == MechanicEntity.MechanicEntity;
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_IsTarget.Value = false;
		ReactiveProperty<AbilityTargetUIData> abilityTargetUIData = m_AbilityTargetUIData;
		AbilityTargetUIData value = (m_AbilityTargetUIInitialData.Value = default(AbilityTargetUIData));
		abilityTargetUIData.Value = value;
		m_IsAoETarget.Value = false;
		m_Ability.Value = null;
		m_IsCaster.Value = false;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateTBMEntity(inCombat);
	}

	public void HandleHighlightChange(bool isOn)
	{
		m_ForceHotKeyPressed.Value = isOn;
	}

	public void HandleUnitInfoVisibilityChange(bool isVisible)
	{
		if (EventInvokerExtensions.MechanicEntity == MechanicEntity.MechanicEntity)
		{
			m_UnitInfoVisible.Value = isVisible;
			if (isVisible)
			{
				MechanicEntity.AdditionalCombatObjective?.SetAsViewed();
			}
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!base.IsDisposed)
		{
			UpdateHiddenConditions();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!base.IsDisposed)
		{
			UpdateHiddenConditions();
		}
	}

	public void HandleFeatureAdded(FeatureCountableFlag feature)
	{
		HandleFeatureInternal(feature);
	}

	public void HandleFeatureRemoved(FeatureCountableFlag feature)
	{
		HandleFeatureInternal(feature);
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		if (EventInvokerExtensions.Entity == MechanicEntity.MechanicEntity)
		{
			m_IsInCombat.Value = true;
		}
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		if (EventInvokerExtensions.Entity == MechanicEntity.MechanicEntity)
		{
			m_IsInCombat.Value = false;
		}
	}

	private void HandleFeatureInternal(FeatureCountableFlag feature)
	{
		if (feature.Type == MechanicsFeatureType.IsUntargetable)
		{
			UpdateHiddenConditions();
		}
	}

	private void UpdateHiddenConditions()
	{
		bool flag = Game.Instance.CurrentModeType == GameModeType.Cutscene;
		bool flag2 = MechanicEntity.Features != null && (bool)MechanicEntity.Features.IsUntargetable;
		m_HasHiddenCondition.Value = flag || flag2;
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		m_IsDeadOrUnconsciousIsDead.Value = MechanicEntity.IsDeadOrUnconscious;
	}

	public void HandleFactionChanged()
	{
		HandleFactionChangedInternal();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		HandleFactionChangedInternal();
	}

	private void HandleFactionChangedInternal()
	{
		if (EventInvokerExtensions.MechanicEntity == MechanicEntity.MechanicEntity)
		{
			DelayedInvoker.InvokeInFrames(UpdateProperties, 1);
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return MechanicEntity.MechanicEntity;
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		m_IsActing.Value = true;
		UpdateConcentration();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		m_CoverState.UpdateCoverType();
		if (command.Executor == MechanicEntity.MechanicEntity)
		{
			m_IsActing.Value = false;
			UpdateConcentration();
			UpdateMorale();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		m_IsActing.Value = false;
		UpdateConcentration();
	}

	public void HandleRoleSet(string entityId)
	{
		MechanicEntityUIWrapper mechanicEntity = MechanicEntity;
		if (mechanicEntity.IsPlayer && mechanicEntity.MechanicEntity != null && MechanicEntity.MechanicEntity.UniqueId == entityId)
		{
			m_Name.Value = MechanicEntity.MechanicEntity.GetUnitNameWithPlayer();
		}
	}

	public void HandleObjectHighlightChange()
	{
		if (EventInvokerExtensions.MapObjectEntity != null && EventInvokerExtensions.MapObjectEntity == MechanicEntity.MechanicEntity)
		{
			m_IsMouseOverUnit.Value = EventInvokerExtensions.MapObjectEntity.View.Highlighted;
		}
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		if (ability.Caster == MechanicEntity.MechanicEntity)
		{
			bool value = Ability.CurrentValue == null && hover && ability.IsAvailable && ability.TargetAnchor == AbilityTargetAnchor.Owner && Game.Instance.Controllers.SelectionCharacter.IsSelected(MechanicEntity.MechanicEntity as BaseUnitEntity);
			m_HoverSelfTargetAbility.Value = value;
		}
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		m_Name.Value = MechanicEntity.MechanicEntity.GetUnitNameWithPlayer();
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity != MechanicEntity.MechanicEntity)
		{
			return;
		}
		m_IsPingUnit.Value = true;
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween?.Kill();
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_IsPingUnit.Value = false;
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				m_IsPingUnit.Value = false;
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}

	public void HandleLootDroppedAsAttached()
	{
		m_HasLoot.Value = true;
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		if (EventInvokerExtensions.MapObjectEntity != null && EventInvokerExtensions.MapObjectEntity == MechanicEntity.MechanicEntity)
		{
			m_IsDeadOrUnconsciousIsDead.Value = MechanicEntity.IsDeadOrUnconscious;
		}
	}

	void IEntityGainFactHandler.HandleEntityGainFact(EntityFact fact)
	{
		UpdateConcentration();
		UpdateMoralePrediction(EventInvokerExtensions.MechanicEntity);
		UpdateMoraleLeading();
	}

	private void UpdateMoraleLeading()
	{
		if (MechanicEntity.MechanicEntity != null)
		{
			m_IsMoraleLeader.Value = MechanicEntity.MechanicEntity.Features.MoraleLeader.Value;
		}
	}

	void IEntityLostFactHandler.HandleEntityLostFact(EntityFact fact)
	{
		UpdateConcentration();
		UpdateMoralePrediction(EventInvokerExtensions.MechanicEntity);
		UpdateMoraleLeading();
	}

	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		UpdateMorale();
	}

	public void HandleMoraleValueChanged(int delta, bool hasCriticalEffect)
	{
		UpdateMorale();
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		m_CoverState.UpdateCoverType(null, position);
	}

	private void UpdateMorale()
	{
		UpdateMoraleLeading();
		m_MoraleChanged.Execute();
	}

	public void OnEventAboutToTrigger(RulePerformMoraleChange evt)
	{
	}

	public void OnEventDidTrigger(RulePerformMoraleChange evt)
	{
		UpdateMoralePrediction(evt.Target);
	}

	private void UpdateMoralePrediction(MechanicEntity target)
	{
		if (target == MechanicEntity.MechanicEntity)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_MoralePrediction.Value = Game.Instance.Controllers.MoraleController.GetMoralePrediction(MechanicEntity.MechanicEntity as BaseUnitEntity);
			}, 1);
		}
	}

	public Transform GetBone()
	{
		if (m_UnitBoneScanned || MechanicEntity.MechanicEntity?.View == null)
		{
			return m_Bone;
		}
		m_Bone = MechanicEntity.MechanicEntity.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
		m_UnitBoneScanned = true;
		return m_Bone;
	}

	private void HandleMeleeAttackHovered(bool isHovered)
	{
		AbilityData currentValue = Ability.CurrentValue;
		if ((object)currentValue != null && currentValue.IsMelee && currentValue.IsSingleTarget && !Ability.CurrentValue.IsPrecise)
		{
			if (!isHovered || Ability.CurrentValue == null)
			{
				ReactiveProperty<AbilityTargetUIData> abilityTargetUIData = m_AbilityTargetUIData;
				AbilityTargetUIData value = (m_AbilityTargetUIInitialData.Value = default(AbilityTargetUIData));
				abilityTargetUIData.Value = value;
				m_IsTarget.Value = false;
			}
			else
			{
				TargetWrapper targetWrapper = GetTargetWrapper();
				bool value2 = targetWrapper != null && Ability.CurrentValue.CanTargetFromDesiredPosition(targetWrapper);
				m_IsTarget.Value = value2;
				HandleAbilityTargetSelectionStart(Ability.CurrentValue);
			}
		}
	}
}
