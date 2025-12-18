using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGainFactionVendorDiscountHandler : ISubscriber
{
	void HandleGainFactionVendorDiscount(FactionType factionType, int discount);
}
