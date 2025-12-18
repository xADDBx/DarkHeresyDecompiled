using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("dbc6e3efabe1d4e4f819632c80ca360e")]
public class WarhammerContextActionDecreaseMilitaryRating : ContextAction
{
	public int Value;

	public bool ToCaster;

	public override string GetCaption()
	{
		return string.Format("Decrease {0} Military Rating by {1}", ToCaster ? "caster" : "target", Value);
	}

	protected override void RunAction()
	{
	}
}
