using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BricksGroupTwoColumnsVM : BricksGroupBaseVM
{
	public BricksGroupTwoColumnsVM(IReadOnlyList<TooltipBrickVM> children, bool hasBackground = false, Color? backgroundColor = null)
		: base(children, hasBackground, backgroundColor)
	{
	}
}
