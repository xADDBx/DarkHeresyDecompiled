using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Concentration.Events;

public interface IChannellingStart : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleChannelingStart();
}
