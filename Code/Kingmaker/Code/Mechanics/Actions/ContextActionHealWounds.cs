using System;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Mechanics.Actions;

[Serializable]
[Obsolete]
[TypeId("f715d518d45e412f9dbf074e4d78c97d")]
public class ContextActionHealWounds : ContextAction
{
	public bool AllowOldWounds;

	public ContextValue Stacks;

	public override string GetCaption()
	{
		return "Heal Wounds" + (AllowOldWounds ? " [fresh and old]" : " [fresh only]") + (Stacks.IsZero ? " (all stacks)" : $" ({Stacks} stacks)");
	}

	protected override void RunAction()
	{
	}
}
