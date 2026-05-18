using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Facts And Buffs/OneUnitTurnBuff")]
[TypeId("a831eee056b245c0aeb142bae796d90f")]
public class OneUnitTurnBuff : UnitBuffComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!ContextData<TurnController.InterruptTurnEndMark>.Current)
		{
			base.Buff.Remove();
		}
	}
}
