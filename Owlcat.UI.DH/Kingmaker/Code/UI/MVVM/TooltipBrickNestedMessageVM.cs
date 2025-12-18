using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickNestedMessageVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly int ShotNumber;

	public readonly MechanicEntity Unit;

	public readonly TooltipBaseTemplate TooltipTemplate;

	public readonly bool NeedShowLine;

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public TooltipBrickNestedMessageVM(string text, Color textColor, PrefixIcon prefixIcon, int shotNumber, MechanicEntity unit, TooltipBaseTemplate tooltipTemplate, bool needShowLine = true)
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
