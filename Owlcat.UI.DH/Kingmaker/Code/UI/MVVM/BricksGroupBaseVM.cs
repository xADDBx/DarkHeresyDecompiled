using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BricksGroupBaseVM : TooltipBrickVM
{
	public readonly IReadOnlyList<TooltipBrickVM> Children;

	public readonly bool HasBackground;

	public readonly Color? BackgroundColor;

	protected BricksGroupBaseVM(IReadOnlyList<TooltipBrickVM> children, bool hasBackground = false, Color? backgroundColor = null)
	{
		Children = children;
		HasBackground = hasBackground;
		BackgroundColor = backgroundColor;
	}
}
