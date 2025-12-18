using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface IEntityDamageChanged : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleDamageChanged(PartHealth health);
}
public interface IEntityDamageChanged<TTag> : IEntityDamageChanged, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityDamageChanged, TTag>
{
}
