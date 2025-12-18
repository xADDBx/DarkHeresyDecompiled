using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface ICellAbilityHandler<TTag> : ICellAbilityHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ICellAbilityHandler, TTag>
{
}
public interface ICellAbilityHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleCellAbility(AbilityTargetUIData abilityTarget);

	void HandleCellAbilityClear();
}
