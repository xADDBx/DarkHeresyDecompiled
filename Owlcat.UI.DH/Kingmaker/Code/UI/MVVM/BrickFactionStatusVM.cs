using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFactionStatusVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public readonly string Status;

	public BrickFactionStatusVM(string label, string status, Sprite icon = null)
	{
		Icon = icon;
		Label = label;
		Status = status;
	}
}
