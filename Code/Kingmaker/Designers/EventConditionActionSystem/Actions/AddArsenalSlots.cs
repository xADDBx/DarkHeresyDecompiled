using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("1397ef3777f84f2597dd2b9f41144f6f")]
public class AddArsenalSlots : GameAction
{
	public override string GetCaption()
	{
		return "Add arsenal slots if needed";
	}

	protected override void RunAction()
	{
	}
}
