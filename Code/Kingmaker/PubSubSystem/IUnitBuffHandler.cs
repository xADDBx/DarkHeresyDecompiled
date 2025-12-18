using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.PubSubSystem;

public interface IUnitBuffHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleBuffDidAdded(Buff buff, MechanicEntity caster);

	void HandleBuffDidRemoved(Buff buff, MechanicEntity caster);

	void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster);

	void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster);
}
public interface IUnitBuffHandler<TTag> : IEventTag<IUnitBuffHandler, TTag>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber
{
}
