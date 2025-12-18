using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("9fa195b8bf73baf49bd8bef276fd8fc0")]
public class WarhammerContextActionIncreaseMilitaryRating : ContextAction
{
	public int Value;

	public override string GetCaption()
	{
		return $"Increase Military Rating by {Value}";
	}

	protected override void RunAction()
	{
	}
}
