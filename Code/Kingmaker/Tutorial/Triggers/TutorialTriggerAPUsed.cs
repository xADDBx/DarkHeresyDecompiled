using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("c3b35e6f79564a7dbce1629136b3ffd4")]
public class TutorialTriggerAPUsed : TutorialTrigger, IUnitActionPointsHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public void HandleRestoreActionPoints()
	{
	}

	public void HandleActionPointsSpent(BaseUnitEntity unit)
	{
		if (unit.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}
}
