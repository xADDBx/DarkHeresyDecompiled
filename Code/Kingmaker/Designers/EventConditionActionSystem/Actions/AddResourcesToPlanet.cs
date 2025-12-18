using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("92acbbc7e734d4140adb8d6b54cbd58b")]
[PlayerUpgraderAllowed(true)]
public class AddResourcesToPlanet : GameAction
{
	[NotNull]
	public ResourceData[] Resources;

	[NotNull]
	public BlueprintStarSystemObjectReference StarSystemObject;

	public override string GetCaption()
	{
		return "Add resources to planet";
	}

	protected override void RunAction()
	{
	}
}
