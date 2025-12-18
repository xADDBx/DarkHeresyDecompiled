using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("5be3ffdc12ddc2a458ff9522c1fefdf2")]
public class ContextActionDealCrewDamage : ContextAction
{
	public int Amount;

	public bool ToCaster;

	public override string GetCaption()
	{
		return "Deal crew damage";
	}

	protected override void RunAction()
	{
	}
}
