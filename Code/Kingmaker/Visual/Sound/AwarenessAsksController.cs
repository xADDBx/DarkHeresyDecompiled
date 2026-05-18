using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class AwarenessAsksController : BaseAsksController, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber
{
	void IAwarenessHandler.OnEntityNoticed(BaseUnitEntity spotter)
	{
		if (spotter.View != null)
		{
			spotter.View.Asks?.Discovery.Schedule();
		}
	}
}
