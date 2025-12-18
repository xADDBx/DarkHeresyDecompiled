using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Gameplay.Parts;

public interface IAbilityUpgradeUpdatedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleAbilityUpgradeUpdated(BlueprintAbility upgradedAbility);
}
