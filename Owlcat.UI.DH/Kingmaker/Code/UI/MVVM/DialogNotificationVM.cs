using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationVM : ViewModel
{
	public static readonly DialogNotificationVM Empty = new DialogNotificationVM(string.Empty);

	public readonly string Label;

	public readonly Sprite Icon;

	public readonly TooltipBaseTemplate IconTooltip;

	public DialogNotificationVM(string label, Sprite icon = null, TooltipBaseTemplate iconTooltip = null)
	{
		Label = label;
		Icon = icon;
		IconTooltip = iconTooltip;
	}
}
