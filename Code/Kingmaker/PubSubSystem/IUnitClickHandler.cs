using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitClickHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleClickOnUnit();
}
