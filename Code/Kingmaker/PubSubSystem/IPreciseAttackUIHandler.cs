using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPreciseAttackUIHandler : ISubscriber
{
	void HandleOpenPreciseAttackInterface(BaseUnitEntity target, bool targetCovered);
}
