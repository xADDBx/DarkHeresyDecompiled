namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IVendorTrashSellStateChangedHandler : ISubscriber
{
	void HandleTrashSellStateChanged();
}
