using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("9d042b002321e7345999f9d905501662")]
[PlayerUpgraderAllowed(false)]
public class StartProjectInColony : GameAction
{
	[ValidateNotNull]
	public BlueprintColonyProjectReference Project;

	public override string GetCaption()
	{
		return "Add project " + Project.Get().Name + " to current colony in context";
	}

	protected override void RunAction()
	{
	}
}
