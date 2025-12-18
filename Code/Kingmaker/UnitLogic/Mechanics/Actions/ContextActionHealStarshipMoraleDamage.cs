using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("fafdce45c270d9547b4d6f26518983dc")]
public class ContextActionHealStarshipMoraleDamage : ContextAction
{
	public int Amount;

	public bool IsRecoverFullMorale;

	public override string GetCaption()
	{
		return "Heal morale damage";
	}

	protected override void RunAction()
	{
	}
}
