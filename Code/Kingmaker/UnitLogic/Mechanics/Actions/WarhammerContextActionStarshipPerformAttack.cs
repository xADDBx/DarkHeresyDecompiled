using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("b68d691de8d312942ba1cda58b61b720")]
public class WarhammerContextActionStarshipPerformAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform an attack with a starship weapon that gave this ability";
	}

	protected override void RunAction()
	{
	}
}
