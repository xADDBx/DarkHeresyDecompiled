using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Heal/HealthGuard")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintDestructibleObject))]
[AllowMultipleComponents]
[TypeId("0dc054950e8a441f85a27a27e021d947")]
public class HealthGuard : MechanicEntityFactComponentDelegate, IActorStatChangedHandler<EntitySubscriber>, IActorStatChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IActorStatChangedHandler, EntitySubscriber>, IEntitySubscriber
{
	[InfoBox("0 means 1 HP")]
	public int HealthPercent;

	protected override void OnActivateOrPostLoad()
	{
		PartHealth optional = base.Owner.GetOptional<PartHealth>();
		if (optional != null)
		{
			int value = Math.Max(1, (int)(0.01 * (double)HealthPercent * (double)optional.MaxHitPoints));
			base.Owner.Features.Immortality.Retain();
			optional.AddHealthGuard(base.Fact, this, value);
		}
	}

	protected override void OnDeactivate()
	{
		PartHealth optional = base.Owner.GetOptional<PartHealth>();
		if (optional != null)
		{
			base.Owner.Features.Immortality.Release();
			optional.RemoveHealthGuard(base.Fact, this);
		}
	}

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		if (stats.Contains(StatType.MaxHitPoints))
		{
			base.Fact.Reapply();
		}
	}
}
