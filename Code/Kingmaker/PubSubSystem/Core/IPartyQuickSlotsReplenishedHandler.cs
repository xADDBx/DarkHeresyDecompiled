using Kingmaker.Code.Framework.GameLog;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPartyQuickSlotsReplenishedHandler : ISubscriber
{
	void HandleQuickSlotsReplenished(QuickSlotsReplenishResult result);
}
