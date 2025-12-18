using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitInfoDetailsUIHandler : ISubscriber
{
	void HandleUnitManual(MechanicEntity mechanicEntity);
}
