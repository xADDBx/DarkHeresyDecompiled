using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("f1966a51ad067e74db86d1ae8300c62b")]
public class WarhammerSelfRemoveBuff : UnitBuffComponentDelegate, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IRoundEndHandler
{
	public bool removeOnNewTurn;

	public bool removeOnEndOfCombat;

	public bool RemoveOnEndOfRound;

	[ShowIf("RemoveOnEndOfRound")]
	public bool RemoveOnlyOnLastRound;

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (removeOnEndOfCombat && !isTurnBased)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (removeOnNewTurn && mechanicEntity == base.Owner)
		{
			base.Buff.Remove();
		}
	}

	public void HandleRoundEnd(bool isTurnBased)
	{
		if (RemoveOnEndOfRound && (!RemoveOnlyOnLastRound || base.Buff.ExpirationInRounds == 1))
		{
			base.Buff.Remove();
		}
	}
}
