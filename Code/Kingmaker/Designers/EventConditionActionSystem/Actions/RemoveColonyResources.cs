using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("62dba2a910a946a7b8092daa3e399e8a")]
[PlayerUpgraderAllowed(false)]
public class RemoveColonyResources : GameAction
{
	[NotNull]
	public ResourceData[] Resources;

	public override string GetCaption()
	{
		return "Remove resources from pool";
	}

	protected override void RunAction()
	{
	}
}
