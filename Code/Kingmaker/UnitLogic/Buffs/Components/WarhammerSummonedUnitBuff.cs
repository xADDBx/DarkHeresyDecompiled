using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[ComponentName("Buffs/Special/Summoned unit")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("01dcaacf0399dc2449b639320aec66cb")]
public class WarhammerSummonedUnitBuff : UnitBuffComponentDelegate, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>
{
	[SerializeField]
	private bool m_ExpireWhenAloneAsStarship;

	[SerializeField]
	private ActionList m_ActionsOnExpiration;

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (base.Buff.Rank > 1)
		{
			base.Buff.RemoveRank();
		}
		else
		{
			base.Fact.RunActionInContext(m_ActionsOnExpiration, base.OwnerTargetWrapper);
		}
	}
}
