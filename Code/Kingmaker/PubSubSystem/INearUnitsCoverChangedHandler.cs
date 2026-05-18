using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INearUnitsCoverChangedHandler : ISubscriber
{
	void HandleNearUnitsCoverProviderRegistered(BaseUnitEntity unit);

	void HandleNearUnitsCoverProviderUnregistered(BaseUnitEntity unit);
}
