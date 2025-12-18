using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("109db0637dd9f8449ad480b8d8bb5451")]
public class ContextActionOutOfCombatHeal : ContextAction
{
	public override string GetCaption()
	{
		return "Deprecated, use ContextActionMedicae instead";
	}

	protected override void RunAction()
	{
		Element.LogError(this, "ContextActionOutOfCombatHeal is deprecated, use ContextActionMedicae instead");
	}
}
