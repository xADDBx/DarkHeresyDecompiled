using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitInfoVisibilityUIHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitInfoVisibilityChange(bool isVisible);
}
