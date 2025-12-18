using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Stats;

public interface IModifiableValueChangedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleModifiableValueChanged(ModifiableValue modifiableValue);
}
public interface IModifiableValueChangedHandler<TTag> : IModifiableValueChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IModifiableValueChangedHandler, TTag>
{
}
