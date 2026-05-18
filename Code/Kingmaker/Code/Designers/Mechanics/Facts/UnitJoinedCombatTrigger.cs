using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Combat/UnitJoinedCombatTrigger")]
[TypeId("153c40c97b454de0927e15f66fb9289a")]
public class UnitJoinedCombatTrigger : UnitFactComponentDelegate, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, ITurnBasedModeHandler
{
	public ActionList Actions;

	public void RunActions()
	{
		using (EvalContext.PushContextMaybe(base.Fact.MaybeContext, base.Owner))
		{
			base.Fact.RunActionInContext(Actions, base.Owner);
		}
	}

	public void HandleUnitJoinCombat()
	{
		RunActions();
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			RunActions();
		}
	}
}
