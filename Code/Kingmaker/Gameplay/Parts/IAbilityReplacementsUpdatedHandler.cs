using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Gameplay.Parts;

public interface IAbilityReplacementsUpdatedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleAbilityReplacementsUpdated(BlueprintAbility target);
}
public interface IAbilityReplacementsUpdatedHandler<TTag> : IAbilityReplacementsUpdatedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityReplacementsUpdatedHandler, TTag>
{
}
