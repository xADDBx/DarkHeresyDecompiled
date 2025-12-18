using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("6e6e1011c5853b14f821d03cd8ee565d")]
public class ExplorePointOfInterest : GameAction
{
	public BlueprintPointOfInterestReference PointOfInterest;

	public override string GetCaption()
	{
		return "Set unique point of interest status to explored";
	}

	protected override void RunAction()
	{
	}
}
