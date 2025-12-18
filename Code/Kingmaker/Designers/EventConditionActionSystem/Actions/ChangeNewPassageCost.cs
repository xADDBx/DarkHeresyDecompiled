using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("65bd45f065b74ef493bbb36e660500ad")]
public class ChangeNewPassageCost : GameAction
{
	public int NewCost;

	public override string GetCaption()
	{
		return "Change cost of creating new passage";
	}

	protected override void RunAction()
	{
	}
}
