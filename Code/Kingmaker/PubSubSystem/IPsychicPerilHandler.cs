using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IPsychicPerilHandler : ISubscriber
{
	void HandlePsychicPeril(RulePerformPsychicPhenomena rule);
}
