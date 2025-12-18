using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.UI;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionPointsVM : ViewModel, IAbilityTargetSelectionUIHandler, ISubscriber, IAbilityTargetHoverUIHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IUnitCommandStartHandler, IUnitCommandActHandler, IUnitRunCommandHandler, IUnitPathManagerHandler, ITurnStartHandler, IInterruptTurnStartHandler, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, IDirectInteractionObjectUIHandler, ITurnBasedModeHandler, IUnitSpentActionPoints, IUnitGainActionPoints, IUnitSpentMovementPoints, IUnitGainMovementPoints
{
	private readonly MechanicEntityUIWrapper m_UnitUIWrapper;

	private AbilityData m_SelectedAbility;

	private AbilityData m_HoveredAbility;

	private AbstractInteractionPart m_Interaction;

	private readonly ReactiveProperty<bool> m_AbilitySelected = new ReactiveProperty<bool>();

	private readonly string m_APColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.ActionPoints) + ">";

	private readonly string m_MPColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.MovePoints) + ">";

	private readonly string m_NotEnoughColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.NotEnoughPoints) + ">";

	private string m_ColorEnd = "</color>";

	private readonly ReactiveProperty<int> m_MaxMP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentMP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CostMP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PredictedMP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_MaxAP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CurrentAP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_CostAP = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PredictedAP = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<int> MaxMP => m_MaxMP;

	public ReadOnlyReactiveProperty<int> CurrentMP => m_CurrentMP;

	public ReadOnlyReactiveProperty<int> CostMP => m_CostMP;

	public ReadOnlyReactiveProperty<int> PredictedMP => m_PredictedMP;

	public ReadOnlyReactiveProperty<int> MaxAP => m_MaxAP;

	public ReadOnlyReactiveProperty<int> CurrentAP => m_CurrentAP;

	public ReadOnlyReactiveProperty<int> CostAP => m_CostAP;

	public ReadOnlyReactiveProperty<int> PredictedAP => m_PredictedAP;

	public ActionPointsVM(MechanicEntity entity)
	{
		m_UnitUIWrapper = new MechanicEntityUIWrapper(entity);
		UpdateActionPointsFromUnit();
		EventBus.Subscribe(this).AddTo(this);
		CostAP.CombineLatest(CostMP, m_AbilitySelected, (int ap, int mp, bool _) => new { ap, mp }).Subscribe(value =>
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.SpaceCombat))
			{
				float mp2 = value.mp;
				int ap2 = Mathf.RoundToInt(value.ap);
				bool noMove = false;
				bool setForce = false;
				AbilityData abilityData = m_SelectedAbility ?? m_HoveredAbility;
				if (abilityData != null)
				{
					mp2 = UnitPathManager.Instance.MemorizedPathCost;
					noMove = abilityData.ClearMPAfterUse;
					setForce = abilityData.TargetAnchor == AbilityTargetAnchor.Owner || m_SelectedAbility != null;
				}
				SetCursorTexts(mp2, ap2, noMove, setForce);
			}
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
	}

	private void SetCursorTexts(float mp, int ap, bool noMove, bool setForce)
	{
		string upperText = null;
		string lowerText = null;
		bool flag = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.IsMyNetRole();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		if (mp > 0f && flag)
		{
			string text = (((float)CurrentMP.CurrentValue >= mp) ? m_MPColor : m_NotEnoughColor);
			upperText = $"{text}<size=150%>{mp}</size>{m_ColorEnd} {tooltips.MP.Text}";
		}
		if (ap > 0 && flag)
		{
			lowerText = $"{m_APColor}<size=150%>{ap}</size>{m_ColorEnd} {tooltips.AP.Text}";
		}
		Game.Instance.CursorController.SetTexts_APMP(upperText, lowerText, setForce);
		Game.Instance.CursorController.SetNoMoveIcon(noMove, setForce);
	}

	private void UpdateActionPointsFromUnit()
	{
		if (m_UnitUIWrapper.MechanicEntity != null && m_UnitUIWrapper.CombatState != null)
		{
			m_MaxMP.Value = Mathf.RoundToInt((m_UnitUIWrapper.CombatState.MovementPoints <= (float)m_UnitUIWrapper.CombatState.MovementPointsMax) ? ((float)m_UnitUIWrapper.CombatState.MovementPointsMax) : m_UnitUIWrapper.CombatState.MovementPoints);
			m_CurrentMP.Value = Mathf.RoundToInt(m_UnitUIWrapper.CombatState.MovementPoints);
			m_PredictedMP.Value = CurrentMP.CurrentValue;
			m_MaxAP.Value = ((m_UnitUIWrapper.CombatState.ActionPoints <= m_UnitUIWrapper.CombatState.ActionPointsMax) ? m_UnitUIWrapper.CombatState.ActionPointsMax : m_UnitUIWrapper.CombatState.ActionPoints);
			m_CurrentAP.Value = m_UnitUIWrapper.CombatState.ActionPoints;
			m_PredictedAP.Value = CurrentAP.CurrentValue;
		}
	}

	private void UpdateActionPointsFromAction()
	{
		if (m_UnitUIWrapper.MechanicEntity != null && m_UnitUIWrapper.CombatState != null)
		{
			m_CurrentMP.Value = Mathf.RoundToInt(m_UnitUIWrapper.CombatState.MovementPoints);
			if (PredictedMP.CurrentValue > CurrentMP.CurrentValue)
			{
				m_PredictedMP.Value = CurrentMP.CurrentValue;
			}
			m_CurrentAP.Value = m_UnitUIWrapper.CombatState.ActionPoints;
			if (PredictedAP.CurrentValue > CurrentAP.CurrentValue)
			{
				m_PredictedAP.Value = CurrentAP.CurrentValue;
			}
		}
	}

	private void SetMPCost(int cost)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && !(m_SelectedAbility != null) && !(m_HoveredAbility != null))
		{
			if (m_Interaction != null)
			{
				ClearMPCost();
				return;
			}
			m_CostMP.Value = cost;
			m_PredictedMP.Value = Mathf.Max(CurrentMP.CurrentValue - CostMP.CurrentValue, 0);
		}
	}

	private void CalculateAPCost()
	{
		CalculateAPCost(m_SelectedAbility ?? m_HoveredAbility);
		CalculateAPCost(m_Interaction);
		if (m_SelectedAbility == null && m_HoveredAbility == null && m_Interaction == null)
		{
			ClearAPCost();
			ClearACCost();
		}
	}

	private void CalculateAPCost(AbilityData ability)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && !(ability == null))
		{
			m_CostMP.Value = (ability.ClearMPAfterUse ? CurrentMP.CurrentValue : 0);
			m_PredictedMP.Value = CurrentMP.CurrentValue - CostMP.CurrentValue;
			m_CostAP.Value = ability.CalculateActionPointCost();
			m_PredictedAP.Value = CurrentAP.CurrentValue - CostAP.CurrentValue;
		}
	}

	private void CalculateAPCost(AbstractInteractionPart interaction)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && interaction != null)
		{
			m_CostAP.Value = interaction.ActionPointsCost;
			m_PredictedAP.Value = CurrentAP.CurrentValue - CostAP.CurrentValue;
		}
	}

	private void ClearAPCost()
	{
		m_CostAP.Value = 0;
		m_PredictedAP.Value = CurrentAP.CurrentValue;
		m_PredictedAP.ForceNotify();
	}

	private void ClearMPCost()
	{
		m_CostMP.Value = 0;
		m_PredictedMP.Value = CurrentMP.CurrentValue;
		m_PredictedAP.ForceNotify();
	}

	private void ClearACCost()
	{
		m_PredictedAP.ForceNotify();
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_SelectedAbility = ability;
		m_AbilitySelected.Value = true;
		CalculateAPCost();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_SelectedAbility = null;
		m_AbilitySelected.Value = false;
		ClearAPCost();
		ClearACCost();
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	public void HandleAbilityTargetMarkerHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	private void HandleAbilityTargetHoverInternal(AbilityData ability, bool hover)
	{
		m_HoveredAbility = (hover ? ability : null);
		CalculateAPCost();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
			ClearMPCost();
			ClearACCost();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandlePathAdded(Path path, float cost, List<BaseUnitEntity> enemiesAoO)
	{
		HandleCurrentNodeChanged(cost);
	}

	public void HandlePathRemoved()
	{
		if (Game.Instance.Controllers.TurnController.IsPlayerTurn)
		{
			SetMPCost(0);
		}
	}

	public void HandleCurrentNodeChanged(float cost)
	{
		if (Game.Instance.Controllers.TurnController.IsPlayerTurn)
		{
			SetMPCost(Mathf.RoundToInt(cost));
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	private void HandleUnitStartTurnInternal()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			UpdateActionPointsFromUnit();
		}
	}

	private bool ShouldHandle(AbstractUnitCommand command)
	{
		return command.Executor == m_UnitUIWrapper.MechanicEntity;
	}

	private bool ShouldHandle()
	{
		return EventInvokerExtensions.MechanicEntity == m_UnitUIWrapper.MechanicEntity;
	}

	public void HandleObjectHighlightChange()
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		if (entity == null || !entity.View.Highlighted)
		{
			m_Interaction = null;
		}
		else
		{
			m_Interaction = entity.Interactions.FirstOrDefault();
		}
		CalculateAPCost();
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void HandleObjectInteract(bool isOn)
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		if (entity == null || !isOn)
		{
			m_Interaction = null;
		}
		else
		{
			m_Interaction = entity.Interactions.FirstOrDefault();
		}
		CalculateAPCost();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		ClearMPCost();
		ClearAPCost();
		ClearACCost();
	}

	public void HandleUnitSpentActionPoints(int actionPointsSpent)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitGainActionPoints(int actionPoints, MechanicsContext context)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitGainMovementPoints(float movementPoints, MechanicsContext context)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}
}
