using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("5fd4ac532f6e1d148af09cb8321fe656")]
public class ContextActionDealStarshipMoraleDamage : ContextAction
{
	public int Amount;

	public bool ToCaster;

	public override string GetCaption()
	{
		return "Deal morale damage";
	}

	protected override void RunAction()
	{
	}
}
