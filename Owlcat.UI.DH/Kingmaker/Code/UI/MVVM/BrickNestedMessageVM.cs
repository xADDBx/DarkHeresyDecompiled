using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickNestedMessageVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly int ShotNumber;

	public readonly MechanicEntity Unit;

	public readonly TooltipBaseTemplate TooltipTemplate;

	public readonly bool NeedShowLine;

	public BrickNestedMessageVM(CombatLogMessage message, bool needShowLine = true)
	{
		Text = message.Message;
		TextColor = message.TextColor;
		PrefixIcon = message.PrefixIcon;
		ShotNumber = message.ShotNumber;
		Unit = message.Unit;
		TooltipTemplate = message.Tooltip;
		NeedShowLine = needShowLine;
	}

	public BrickNestedMessageVM(string text, Color textColor, PrefixIcon prefixIcon, int shotNumber, MechanicEntity unit, TooltipBaseTemplate tooltipTemplate, bool needShowLine = true)
	{
		Text = text;
		TextColor = textColor;
		PrefixIcon = prefixIcon;
		ShotNumber = shotNumber;
		Unit = unit;
		TooltipTemplate = tooltipTemplate;
		NeedShowLine = needShowLine;
	}
}
