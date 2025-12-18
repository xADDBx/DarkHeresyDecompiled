using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("8c209d83de1b09841858483ae25327e8")]
public class TutorialTriggerDamagedAfterCombat : TutorialTrigger, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit.Health.HitPointsLeft != unit.Health.MaxHitPoints)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
			});
		}
	}
}
