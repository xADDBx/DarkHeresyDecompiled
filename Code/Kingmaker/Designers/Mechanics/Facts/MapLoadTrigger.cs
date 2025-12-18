using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d07bb3806a12fda4aa2d4a7fc66b892f")]
public class MapLoadTrigger : UnitFactComponentDelegate, IPartyLeaveAreaHandler, ISubscriber
{
	public ActionList AreaUnloadActions;

	public void HandlePartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea)
	{
		if (currentArea == targetArea.Area)
		{
			return;
		}
		using (base.Fact.MaybeContext?.SetScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(AreaUnloadActions, base.Owner.ToITargetWrapper());
		}
	}
}
