using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("de7e7ea46548a14438b6ce6e738ca309")]
public class WarhammerContextActionCoverActions : ContextAction
{
	public ActionList NoCoverActions;

	public ActionList CoverActions;

	public override string GetCaption()
	{
		return "Do actions depending on cover";
	}

	protected override void RunAction()
	{
	}
}
