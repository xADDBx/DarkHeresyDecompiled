using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("018ffdf3c815bfa4189c0fc4cf1c9b19")]
public class RecalculateOnStatChange : UnitFactComponentDelegate, IActorStatChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public StatType Stat;

	public bool CheckCaster;

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		MechanicEntity mechanicEntity2 = (CheckCaster ? base.Context.MaybeCaster : base.Owner);
		if (mechanicEntity == mechanicEntity2 && stats.Contains(Stat))
		{
			base.Fact.Reapply();
		}
	}
}
