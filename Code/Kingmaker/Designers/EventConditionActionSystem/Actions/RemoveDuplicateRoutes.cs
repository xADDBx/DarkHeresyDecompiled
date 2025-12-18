using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("07881642342e41fc85fc10eb70eb3261")]
public class RemoveDuplicateRoutes : GameAction
{
	public override string GetCaption()
	{
		return "Remove duplicate routes";
	}

	protected override void RunAction()
	{
	}
}
