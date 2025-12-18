using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete("Unused")]
[TypeId("9b7f1fac20b15ba4cab1b08d06e674b9")]
public class WarhammerResetStrangulate : ContextAction
{
	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return "Stops all current strangulate effects";
	}
}
