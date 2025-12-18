using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityStatVM : ViewModel
{
	public string AbilityStatValue { get; private set; }

	public Sprite AbilityStatIcon { get; private set; }

	public TooltipBaseTemplate AbilityStatTooltip { get; private set; }

	public CharInfoAbilityStatVM(string value, Sprite icon, TooltipBaseTemplate tooltip)
	{
		AbilityStatValue = value;
		AbilityStatIcon = icon;
		AbilityStatTooltip = tooltip;
	}
}
