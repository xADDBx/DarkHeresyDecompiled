using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("072a56da3cd48a1499c6ccaf28d5eb54")]
public class RemoveBuffIfPartyNotInCombat : UnitBuffComponentDelegate, IPartyCombatHandler, ISubscriber
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			base.Buff.MarkExpired();
		}
	}
}
