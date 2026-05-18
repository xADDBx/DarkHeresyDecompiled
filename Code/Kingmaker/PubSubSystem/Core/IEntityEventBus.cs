using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntityEventBus
{
	EventBusSubscription Subscribe(object subscriber);

	void Unsubscribe(object subscriber);

	void ClearEntitySubscriptions(IEntity entity);

	void RaiseEvent<T>(Action<T> action, bool isCheckRuntime = true) where T : ISubscriber;

	void RaiseEvent<T>(IEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IEntity>;

	void RaiseEvent<T>(IMechanicEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMechanicEntity>;

	void RaiseEvent<T>(IAbstractUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAbstractUnitEntity>;

	void RaiseEvent<T>(IBaseUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IBaseUnitEntity>;

	void RaiseEvent<T>(IAreaEffectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAreaEffectEntity>;

	void RaiseEvent<T>(IMapObjectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMapObjectEntity>;

	void RaiseEvent<T>(IItemEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IItemEntity>;

	void RaiseEvent<T, TInvoker>(TInvoker entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<TInvoker> where TInvoker : IEntity;

	void OnEventAboutToTrigger<T>(T evt) where T : IRulebookEvent;

	void OnEventDidTrigger<T>(T evt) where T : IRulebookEvent;
}
