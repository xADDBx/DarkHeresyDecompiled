using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickNestedMessage : ITooltipBrick
{
	private readonly string m_Text;

	private readonly Color m_TextColor;

	private readonly PrefixIcon m_PrefixIcon;

	private readonly int m_ShotNumber;

	private readonly MechanicEntity m_Unit;

	private readonly TooltipBaseTemplate m_TooltipTemplate;

	private readonly bool m_NeedShowLine;

	public TooltipBrickNestedMessage(CombatLogMessage message, bool needShowLine = true)
	{
		m_Text = message.Message;
		m_TextColor = message.TextColor;
		m_PrefixIcon = message.PrefixIcon;
		m_ShotNumber = message.ShotNumber;
		m_Unit = message.Unit;
		m_TooltipTemplate = message.Tooltip;
		m_NeedShowLine = needShowLine;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickNestedMessageVM(m_Text, m_TextColor, m_PrefixIcon, m_ShotNumber, m_Unit, m_TooltipTemplate, m_NeedShowLine);
	}
}
