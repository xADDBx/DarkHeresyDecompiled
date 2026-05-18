using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICharInfoAbilitiesOpenHandler : ISubscriber
{
	void HandleCharInfoAbilitiesOpen(BaseUnitEntity unit);
}
