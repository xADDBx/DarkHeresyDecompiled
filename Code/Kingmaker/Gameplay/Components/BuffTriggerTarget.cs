using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("1d44512f77a7457fa4f76335dcbc5f6f")]
public sealed class BuffTriggerTarget : BuffTrigger, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber
{
	void IUnitBuffHandler.HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		HandleEvent(EventType.Added, buff, caster);
	}

	void IUnitBuffHandler.HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		HandleEvent(EventType.Removed, buff, caster);
	}

	void IUnitBuffHandler.HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
		HandleEvent(EventType.RankIncreased, buff, delta, caster);
	}

	void IUnitBuffHandler.HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
		HandleEvent(EventType.RankDecreased, buff, delta, caster);
	}
}
