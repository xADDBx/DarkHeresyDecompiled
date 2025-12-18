using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IRefreshVisibleCollectionHandler : ISubscriber
{
	void Refresh();
}
