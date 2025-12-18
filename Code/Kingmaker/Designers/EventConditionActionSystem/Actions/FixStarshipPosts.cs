using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("026c1d19a5304a40ae7322406f9e2cf8")]
public class FixStarshipPosts : GameAction
{
	public override string GetCaption()
	{
		return "Fix starship posts";
	}

	protected override void RunAction()
	{
	}
}
