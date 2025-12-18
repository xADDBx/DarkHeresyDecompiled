using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IHologramPositionChangedHandler : ISubscriber
{
	void HandleHologramPositionChanged();
}
