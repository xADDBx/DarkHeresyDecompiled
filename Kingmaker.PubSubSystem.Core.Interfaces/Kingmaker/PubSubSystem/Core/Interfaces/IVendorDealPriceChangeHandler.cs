namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IVendorDealPriceChangeHandler : ISubscriber
{
	void HandleDealPriceChanged();

	void HandleVendorPriceChanged();

	void HandlePlayerPriceChanged();
}
