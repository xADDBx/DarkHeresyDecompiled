using System;
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
[TypeId("820d299f01794e10bac98f5908e4a2cd")]
public class CompanionUnconsciousTrigger : UnitFactComponentDelegate, IUnitDeathHandler, ISubscriber
{
	public ActionList Actions;

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity != base.Owner && unitEntity != null && unitEntity.IsInPlayerParty)
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}
}
