using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ISubscriber<out TInvoker> : ISubscriber where TInvoker : IEntity
{
}
