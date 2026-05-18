using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MoraleBalanceVM : ViewModel, IPowerBalanceHandler, ISubscriber, IMoraleVictoryConfirmationRequest, IMoraleVictoryConfirmationHandler, ITurnBasedModeHandler
{
	private readonly ReactiveProperty<float> m_MoraleBalanceNormalized;

	private readonly ReactiveProperty<MoraleBalanceState> m_MoraleVictoryState;

	private readonly float m_ShatteredMultiplierFactor;

	private MoraleGroup m_PlayerGroup;

	private TooltipBaseTemplate m_Tooltip;

	private MoraleController MoraleController => Game.Instance.Controllers.MoraleController;

	public ReadOnlyReactiveProperty<float> MoraleBalanceNormalized => m_MoraleBalanceNormalized;

	public ReadOnlyReactiveProperty<MoraleBalanceState> MoraleVictoryState => m_MoraleVictoryState;

	public MoraleBalanceVM()
	{
		float shatteredPowerBalanceMultiplier = MoraleRoot.Instance.ShatteredPowerBalanceMultiplier;
		m_ShatteredMultiplierFactor = (shatteredPowerBalanceMultiplier - 1f) / shatteredPowerBalanceMultiplier;
		m_MoraleVictoryState = new ReactiveProperty<MoraleBalanceState>().AddTo(this);
		m_MoraleBalanceNormalized = new ReactiveProperty<float>(CalculateBalance()).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public TooltipBaseTemplate GetTooltip(TMP_StyleSheet styleSheet, Color textColor)
	{
		if (m_Tooltip == null)
		{
			m_Tooltip = new TooltipTemplateMoralePressure(m_MoraleBalanceNormalized, m_MoraleVictoryState, styleSheet, textColor);
		}
		return m_Tooltip;
	}

	void IPowerBalanceHandler.HandlePowerBalanceRecalculated()
	{
		m_MoraleBalanceNormalized.Value = CalculateBalance();
	}

	void IPowerBalanceHandler.HandlePowerBalanceValueUpdate(MoraleGroup combatGroup)
	{
	}

	void IPowerBalanceHandler.HandlePowerBalanceStateUpdate(MoraleGroup combatGroup, PowerBalanceState state)
	{
	}

	void IMoraleVictoryConfirmationRequest.HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		m_MoraleVictoryState.Value = MoraleBalanceState.Pending;
		m_MoraleBalanceNormalized.Value = CalculateBalance();
	}

	void IMoraleVictoryConfirmationHandler.HandleMoraleVictoryConfirmation(bool confirmed)
	{
		if (!confirmed)
		{
			m_MoraleVictoryState.Value = MoraleBalanceState.Continued;
			m_MoraleBalanceNormalized.Value = CalculateBalance();
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			m_MoraleVictoryState.Value = MoraleBalanceState.None;
			m_PlayerGroup = null;
			m_MoraleBalanceNormalized.Value = 0f;
		}
	}

	private float CalculateBalance()
	{
		if (m_PlayerGroup == null)
		{
			m_PlayerGroup = MoraleController.MoraleGroups.FirstOrDefault((MoraleGroup g) => g.IsPlayerGroup);
		}
		if (m_PlayerGroup == null)
		{
			Debug.LogError("MoraleBalanceVM: player group is null");
			return 0f;
		}
		float powerValue = m_PlayerGroup.PowerValue;
		float mostPowerfulEnemy = m_PlayerGroup.MostPowerfulEnemy;
		if (float.IsNaN(powerValue) || float.IsNaN(mostPowerfulEnemy) || powerValue < 0f)
		{
			return 0f;
		}
		float num = powerValue * m_ShatteredMultiplierFactor + mostPowerfulEnemy;
		if (Mathf.Approximately(num, 0f))
		{
			return 0f;
		}
		float num2 = powerValue / num;
		float max = ((m_MoraleVictoryState.Value == MoraleBalanceState.None) ? 0.99f : 1f);
		return Mathf.Clamp(num2 * num2, 0f, max);
	}
}
