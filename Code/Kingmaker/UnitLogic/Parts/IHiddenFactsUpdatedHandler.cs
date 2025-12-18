using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface IHiddenFactsUpdatedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleHiddenFactsUpdated(IEnumerable<BlueprintFact> updatedFacts);
}
public interface IHiddenFactsUpdatedHandler<TTag> : IHiddenFactsUpdatedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IHiddenFactsUpdatedHandler, TTag>
{
}
