using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("e77eb1db9e344376b35e4c4f0f5a0b69")]
public class OwnerJoinTBCombatTrigger : MechanicEntityFactComponentDelegate, IEntityJoinTBCombat<EntitySubscriber>, IEntityJoinTBCombat, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityJoinTBCombat, EntitySubscriber>
{
	public RestrictionCalculator Restriction;

	public ActionList Action;

	public void HandleEntityJoinTBCombat()
	{
		if (Restriction.IsPassed(base.Context))
		{
			base.Fact.RunActionInContext(Action);
		}
	}
}
