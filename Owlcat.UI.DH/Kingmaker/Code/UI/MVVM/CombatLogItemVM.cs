using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatLogItemVM : CombatLogBaseVM
{
	public readonly string MessageText;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly int ShotNumber;

	public readonly MechanicEntity Unit;

	public readonly TooltipBaseTemplate TooltipTemplate;

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CombatLogItemVM(CombatLogMessage message)
		: base(message)
	{
		MessageText = message.Message;
		TextColor = message.TextColor;
		PrefixIcon = message.PrefixIcon;
		ShotNumber = message.ShotNumber;
		Unit = message.Unit;
		TooltipTemplate = message.Tooltip;
	}
}
