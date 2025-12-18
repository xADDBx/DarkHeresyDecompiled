using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVeilDamageHandler : ISubscriber
{
	void HandleVeilDamageChanged(int delta, int value);
}
