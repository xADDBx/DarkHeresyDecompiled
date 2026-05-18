using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPickLockHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePickLockSuccess(MapObjectEntity mapObject);

	void HandlePickLockFail(MapObjectEntity mapObject, bool critical);
}
