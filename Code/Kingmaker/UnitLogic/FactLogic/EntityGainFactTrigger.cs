using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[ComponentName("Facts And Buffs/EntityGainFactTrigger")]
[TypeId("eb92fb96ff234caca266feb01a80878e")]
public class EntityGainFactTrigger : MechanicEntityFactComponentDelegate, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public RestrictionCalculator Restriction;

	public ActionList Action;

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact != null)
		{
			MechanicsContext maybeContext = fact.MaybeContext;
			if (maybeContext != null && Restriction.IsPassed(new PropertyContext(base.Owner, maybeContext)))
			{
				base.Fact.RunActionInContext(Action);
			}
		}
	}
}
