using Kingmaker.Controllers;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDynamicCoverProviderChangedHandler : ISubscriber
{
	void HandleDynamicCoverProviderRegistered(IDynamicCoverProvider provider);

	void HandleDynamicCoverProviderUnregistered(IDynamicCoverProvider provider);
}
