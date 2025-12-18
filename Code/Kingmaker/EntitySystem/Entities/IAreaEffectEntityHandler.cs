using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Entities;

public interface IAreaEffectEntityHandler : ISubscriber<IAreaEffectEntity>, ISubscriber
{
	void HandleEntityEnterAreaEffect(MechanicEntity entity);

	void HandleEntityExitAreaEffect(MechanicEntity entity);
}
public interface IAreaEffectEntityHandler<TTag> : IAreaEffectEntityHandler, ISubscriber<IAreaEffectEntity>, ISubscriber, IEventTag<IAreaEffectEntityHandler, TTag>
{
}
