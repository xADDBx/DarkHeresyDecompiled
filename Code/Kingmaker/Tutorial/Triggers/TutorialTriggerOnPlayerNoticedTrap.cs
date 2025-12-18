using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("90127c5db4d24e988646d8f78a45d9ce")]
public class TutorialTriggerOnPlayerNoticedTrap : TutorialTrigger, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber
{
	public void OnEntityNoticed(BaseUnitEntity spotter)
	{
		if (spotter.IsPlayerFaction && EventInvokerExtensions.Entity?.View is TrapObjectView)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = spotter;
			});
		}
	}
}
