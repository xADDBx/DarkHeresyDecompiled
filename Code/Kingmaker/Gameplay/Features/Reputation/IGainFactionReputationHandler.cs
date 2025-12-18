using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Reputation;

public interface IGainFactionReputationHandler : ISubscriber
{
	void HandleGainFactionReputation(FactionType factionType, ReputationType reputationType, int count);
}
