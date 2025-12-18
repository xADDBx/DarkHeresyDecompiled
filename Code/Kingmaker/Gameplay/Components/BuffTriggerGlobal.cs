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
[TypeId("454dde88e08549adba49d32ae1ff52f0")]
public sealed class BuffTriggerGlobal : BuffTrigger, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
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
