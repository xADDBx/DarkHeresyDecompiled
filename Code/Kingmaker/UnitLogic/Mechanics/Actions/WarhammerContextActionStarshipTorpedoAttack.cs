using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("a2e56ff6906d4314197e5410724905c6")]
public class WarhammerContextActionStarshipTorpedoAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform an attack with a starship torpedo weapon that gave this ability";
	}

	protected override void RunAction()
	{
	}
}
