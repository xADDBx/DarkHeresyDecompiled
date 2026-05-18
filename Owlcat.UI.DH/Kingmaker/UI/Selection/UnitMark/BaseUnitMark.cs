using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public abstract class BaseUnitMark : RegisteredBehaviour, IBaseUnitMark, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitSizeHandler<EntitySubscriber>, IUnitSizeHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<IUnitSizeHandler, EntitySubscriber>, IDialogCueHandler, IUnitHighlightUIHandler, IInteractionHighlightUIHandler, IGameModeHandler, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEventTag<ITurnStartHandler, EntitySubscriber>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, IEventTag<ITurnEndHandler, EntitySubscriber>, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IUIVisibilityHandler, ILateUpdatable, IDialogFinishHandler, IAbilityTargetSelectionUIHandler, IMoralePhaseHandler<EntitySubscriber>, IMoralePhaseHandler, IEventTag<IMoralePhaseHandler, EntitySubscriber>
{
	private static readonly int _Color = Shader.PropertyToID("_BaseColor");

	public AbstractUnitEntity Unit { get; private set; }

	public UnitMarkState State { get; protected set; }

	protected static bool IsCutscene => Game.Instance.CurrentModeType == GameModeType.Cutscene;

	protected static bool IsHideAllUI => UIVisibilityState.VisibilityPreset.CurrentValue == UIVisibilityFlags.None;

	public IEntity GetSubscribingEntity()
	{
		return Unit;
	}

	public virtual void Initialize(AbstractUnitEntity unit)
	{
		Unit = unit;
		if (Math.Abs(unit.Corpulence) < Mathf.Epsilon)
		{
			PFLog.UI.Log("Non initialized unit: " + unit, unit.View);
		}
		HandleStateChanged();
		UpdateUnitCurrentTurnState(Unit == Game.Instance.Controllers?.TurnController?.CurrentUnit);
		HandleUnitSizeChanged();
		if (Unit != null)
		{
			if (base.isActiveAndEnabled)
			{
				EventBus.Subscribe(this);
			}
			UpdateCombatState();
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if (Unit != null)
		{
			EventBus.Subscribe(this);
			HandleUnitSizeChanged();
		}
		HandleStateChanged();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		EventBus.Unsubscribe(this);
	}

	public void HandleOnCueShow(CueShowData cueShowData)
	{
		BaseUnitEntity currentSpeaker = Game.Instance.Controllers.DialogController.CurrentSpeaker;
		if (currentSpeaker != null)
		{
			bool active = Unit == currentSpeaker && Game.Instance.CurrentModeType == GameModeType.Dialog;
			SetState(UnitMarkState.DialogCurrentSpeaker, active);
		}
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		SetState(UnitMarkState.DialogCurrentSpeaker, active: false);
	}

	public void HandleUnitJoinCombat()
	{
		UpdateCombatState();
	}

	private void UpdateCombatState()
	{
		SetState(UnitMarkState.IsInCombat, Unit.IsInCombat);
		if (Unit is BaseUnitEntity unit)
		{
			if (!Game.Instance.Controllers.TurnController.InCombat)
			{
				SetState(UnitMarkState.CurrentTurn, active: false);
			}
			else if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
			{
				bool active = Game.Instance.Controllers.SelectionCharacter.IsSelected(unit);
				SetState(UnitMarkState.CurrentTurn, active);
			}
		}
	}

	public void HandleUnitLeaveCombat()
	{
		UpdateCombatState();
	}

	public void HandleUnitSizeChanged()
	{
		HandleUnitSizeChangedImpl();
	}

	protected virtual void HandleUnitSizeChangedImpl()
	{
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Unit.Size);
		base.transform.localScale = new Vector3(rectForSize.Width, 1f, rectForSize.Height);
		base.transform.rotation = Quaternion.identity;
	}

	public virtual void Selected(bool isSelected)
	{
		SetState(UnitMarkState.Selected, isSelected);
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			SetState(UnitMarkState.CurrentTurn, isSelected);
		}
	}

	protected void SetState(UnitMarkState state, bool active)
	{
		UnitMarkState state2 = State;
		state2 = ((!active) ? (state2 & ~state) : (state2 | state));
		if (State != state2)
		{
			State = state2;
			HandleStateChanged();
		}
	}

	public abstract void HandleStateChanged();

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateUnitCurrentTurnState(currentTurn: true);
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		UpdateUnitCurrentTurnState(currentTurn: false);
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateUnitCurrentTurnState(Unit == Game.Instance.Controllers.TurnController.CurrentUnit);
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		UpdateUnitCurrentTurnState(Unit == Game.Instance.Controllers.TurnController.CurrentUnit);
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateCombatState();
		if (!isTurnBased)
		{
			ResetMoralePhase();
		}
	}

	private void UpdateUnitCurrentTurnState(bool currentTurn)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			SetState(UnitMarkState.CurrentTurn, currentTurn);
		}
	}

	void IUnitHighlightUIHandler.HandleHighlightChange(AbstractUnitEntityView unit)
	{
		SetState(UnitMarkState.MouseHovered, unit == Unit.View && unit.MouseHoverHighlighting);
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		SetState(UnitMarkState.Highlighted, isOn);
	}

	void IGameModeHandler.OnGameModeStart(GameModeType gameMode)
	{
		HandleStateChanged();
	}

	void IGameModeHandler.OnGameModeStop(GameModeType gameMode)
	{
	}

	void IUIVisibilityHandler.HandleUIVisibilityChange(UIVisibilityFlags flags)
	{
		HandleStateChanged();
	}

	public abstract void HandleAbilityTargetSelectionStart(AbilityData ability);

	public abstract void HandleAbilityTargetSelectionEnd(AbilityData ability);

	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		ResetMoralePhase();
		switch (phase)
		{
		case MoralePhaseType.Heroic:
			SetState(UnitMarkState.Heroic, active: true);
			break;
		case MoralePhaseType.Broken:
			SetState(UnitMarkState.Broken, active: true);
			break;
		}
	}

	private void ResetMoralePhase()
	{
		SetState(UnitMarkState.Heroic, active: false);
		SetState(UnitMarkState.Broken, active: false);
	}

	public virtual void SetGamepadSelected(bool selected)
	{
	}

	public void DoLateUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	protected bool IsHiddenBySettings()
	{
		if (Unit == null || Unit.IsDisposed || !Unit.IsVisibleForPlayer)
		{
			return true;
		}
		return Unit.Blueprint.GetComponent<UnitUISettings>()?.MarkSettings.HideUnitMark ?? false;
	}

	private void OnDestroy()
	{
		Dispose();
	}
}
