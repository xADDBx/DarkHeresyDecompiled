using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("643f5d612030b3e45ae8775296bbb8d5")]
public class WarhammerContextActionStarshipPBAEAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform PBAE attack with a starship weapon";
	}

	protected override void RunAction()
	{
	}
}
