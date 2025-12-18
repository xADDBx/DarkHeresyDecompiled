using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IHologramClearHandler : ISubscriber
{
	void HandleHologramClear();
}
