using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IForcedPsychicPhenomenaHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleForcedPsychicPhenomena(bool isPerilsOfTheWarp);
}
