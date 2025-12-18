using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.EntitySystem;

public interface IEntityHoldingStateChangedHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleEntityHoldingStateChanged();
}
