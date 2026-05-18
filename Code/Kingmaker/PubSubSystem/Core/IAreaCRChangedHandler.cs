using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAreaCRChangedHandler : ISubscriber
{
	void HandleAreaCRChanged();
}
