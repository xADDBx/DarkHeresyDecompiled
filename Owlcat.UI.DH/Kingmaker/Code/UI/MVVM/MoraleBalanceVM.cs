using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MoraleBalanceVM : ViewModel, IPowerBalanceHandler, ISubscriber, IMoraleVictoryConfirmationRequest, ITurnBasedModeHandler
{
	private readonly MoraleGroup m_PlayerGroup;

	private readonly ReactiveProperty<float> m_MoraleBalanceNormalized;

	private readonly float m_ShatteredMultiplier;

	private bool m_IsMoraleVictoryAchieved;

	private TooltipBaseTemplate m_DefaultTooltip;

	private TooltipBaseTemplate m_VictoryTooltip;

	private MoraleController MoraleController => Game.Instance.Controllers.MoraleController;

	public ReadOnlyReactiveProperty<float> MoraleBalanceNormalized => m_MoraleBalanceNormalized;

	public MoraleBalanceVM()
	{
		m_PlayerGroup = MoraleController.MoraleGroups.FirstOrDefault((MoraleGroup g) => g.IsPlayerGroup);
		m_ShatteredMultiplier = MoraleRoot.Instance.ShatteredPowerBalanceMultiplier;
		m_MoraleBalanceNormalized = new ReactiveProperty<float>().AddTo(this);
		m_MoraleBalanceNormalized.Value = CalculateBalance(m_PlayerGroup);
		EventBus.Subscribe(this).AddTo(this);
	}

	public TooltipBaseTemplate GetDefaultHintTooltipTemplate()
	{
		if (m_DefaultTooltip != null)
		{
			return m_DefaultTooltip;
		}
		HUDTexts hUDTexts = UIStrings.Instance.HUDTexts;
		return m_DefaultTooltip = new TooltipTemplateSimple(hUDTexts.MoraleBalanceHeader, hUDTexts.MoraleBalanceDescription);
	}

	public TooltipBaseTemplate GetVictoryHintTooltipTemplate(Color highlightColor, int highlightSize)
	{
		if (m_VictoryTooltip != null)
		{
			return m_VictoryTooltip;
		}
		HUDTexts hUDTexts = UIStrings.Instance.HUDTexts;
		string arg = ColorUtility.ToHtmlStringRGB(highlightColor);
		string text = hUDTexts.MoraleBalanceVictoryDescription.Text;
		string description = string.Concat(hUDTexts.MoraleBalanceDescription, "\n\n", $"<b><color=#{arg}><size={highlightSize}%>{text}</size></color></b>");
		return m_VictoryTooltip = new TooltipTemplateSimple(hUDTexts.MoraleBalanceHeader, description);
	}

	void IPowerBalanceHandler.HandlePowerBalanceRecalculated()
	{
		m_MoraleBalanceNormalized.Value = CalculateBalance(m_PlayerGroup);
	}

	void IPowerBalanceHandler.HandlePowerBalanceValueUpdate(MoraleGroup combatGroup)
	{
	}

	void IPowerBalanceHandler.HandlePowerBalanceStateUpdate(MoraleGroup combatGroup, PowerBalanceState state)
	{
	}

	void IMoraleVictoryConfirmationRequest.HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		m_IsMoraleVictoryAchieved = true;
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			m_IsMoraleVictoryAchieved = false;
		}
	}

	private float CalculateBalance(MoraleGroup moraleGroup)
	{
		try
		{
			float powerValue = moraleGroup.PowerValue;
			float mostPowerfulEnemy = moraleGroup.MostPowerfulEnemy;
			float num = (m_ShatteredMultiplier - 1f) / m_ShatteredMultiplier;
			float num2 = powerValue / (powerValue * num + mostPowerfulEnemy);
			num2 = Mathf.Clamp01(num2 * num2);
			return m_IsMoraleVictoryAchieved ? num2 : Mathf.Min(num2, 0.99f);
		}
		catch (DivideByZeroException exception)
		{
			Debug.LogException(exception);
			return 0f;
		}
	}
}
