using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEntityDestructionHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleEntityDestroyed();
}
