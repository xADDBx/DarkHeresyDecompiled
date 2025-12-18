using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Gameplay.Features.Morale;

public interface IMoraleChangeRawValueCalculatedTrigger : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleMoraleChangeRawValueCalculated(RulePerformMoraleChange rule);
}
public interface IMoraleChangeRawValueCalculatedTrigger<TTag> : IMoraleChangeRawValueCalculatedTrigger, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IMoraleChangeRawValueCalculatedTrigger, TTag>
{
}
