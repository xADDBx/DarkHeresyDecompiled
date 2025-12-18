using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("4306440a32ed467cb035876eb8223c3d")]
[PlayerUpgraderAllowed(false)]
public class AddStarSystemToVisit : GameAction
{
	public BlueprintStarSystemMapReference StarSystemMap;

	public override string GetCaption()
	{
		return "Add StarSystem " + StarSystemMap.NameSafe() + " To Visit when warp travel to it";
	}

	protected override void RunAction()
	{
	}
}
