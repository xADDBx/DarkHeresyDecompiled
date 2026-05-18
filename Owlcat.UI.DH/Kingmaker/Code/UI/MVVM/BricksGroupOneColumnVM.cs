using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BricksGroupOneColumnVM : BricksGroupBaseVM
{
	public BricksGroupOneColumnVM(IReadOnlyList<TooltipBrickVM> children, bool hasBackground = false, Color? backgroundColor = null)
		: base(children, hasBackground, backgroundColor)
	{
	}
}
