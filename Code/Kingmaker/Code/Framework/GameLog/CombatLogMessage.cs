using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class CombatLogMessage
{
	public readonly DateTime Received = DateTime.Now;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly bool IsSeparator;

	public readonly GameLogEventAddSeparator.States SeparatorState;

	public readonly bool IsPerformAttackMessage;

	private readonly MechanicEntity m_Unit;

	public string Message { get; private set; }

	public int ShotNumber { get; private set; }

	public MechanicEntity Unit => m_Unit;

	public CombatLogMessage(CombatLogMessage message, TooltipBaseTemplate template = null, bool hasTooltip = true, MechanicEntity unit = null)
		: this(message.Message, message.TextColor, message.PrefixIcon, template, hasTooltip, message.ShotNumber, isSeparator: false, GameLogEventAddSeparator.States.Finish, isPerformAttackMessage: false, unit)
	{
	}

	public CombatLogMessage(string message, Color textColor, PrefixIcon icon, TooltipBaseTemplate template = null, bool hasTooltip = true, int shotNumber = 0, bool isSeparator = false, GameLogEventAddSeparator.States separatorState = GameLogEventAddSeparator.States.Finish, bool isPerformAttackMessage = false, MechanicEntity unit = null)
	{
		TextColor = textColor;
		Message = message;
		PrefixIcon = icon;
		ShotNumber = shotNumber;
		IsSeparator = isSeparator;
		SeparatorState = separatorState;
		IsPerformAttackMessage = isPerformAttackMessage;
		if (unit != null && !(unit is MapObjectEntity))
		{
			m_Unit = unit;
		}
		if (hasTooltip || template != null)
		{
			Tooltip = template;
		}
	}

	public CombatLogMessage(string message, bool isSeparator, GameLogEventAddSeparator.States separatorState)
	{
		Message = message;
		IsSeparator = isSeparator;
		SeparatorState = separatorState;
	}

	public void SetShotNumber(int value)
	{
		ShotNumber = value;
	}

	public void ReplaceMessage(string message)
	{
		Message = message;
	}
}
