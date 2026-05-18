using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

[SkipAnalysis]
public static class EventBus
{
	public sealed class OverrideShared : SimpleContextData<EventBusInstance, OverrideShared>
	{
	}

	private static readonly EventBusInstance _Shared = new EventBusInstance();

	public static bool DebugSubscriptions { get; set; }

	[NotNull]
	public static EventBusInstance Shared => SimpleContextData<EventBusInstance, OverrideShared>.Current ?? _Shared;

	[NotNull]
	public static SubscriptionManager<ISubscriber> GlobalSubscribers => Shared.GlobalSubscribers;

	[NotNull]
	public static PooledDictionary<IEntity, SubscriptionManager<ISubscriber>> EntitySubscribers => Shared.EntitySubscribers;

	public static EventBusSubscription Subscribe([CanBeNull] object subscriber)
	{
		return Shared.Subscribe(subscriber);
	}

	public static void Unsubscribe([CanBeNull] object subscriber)
	{
		Shared.Unsubscribe(subscriber);
	}

	public static void ClearEntitySubscriptions([NotNull] IEntity entity)
	{
		Shared.ClearEntitySubscriptions(entity);
	}

	public static bool IsGloballySubscribed(object subscriber)
	{
		return Shared.IsGloballySubscribed(subscriber);
	}

	public static void LogActiveSubscribers()
	{
		Shared.LogActiveSubscribers();
	}

	public static void RaiseEvent<T>(Action<T> action, bool isCheckRuntime = true) where T : ISubscriber
	{
		Shared.RaiseEvent(action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IMechanicEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMechanicEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IAbstractUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAbstractUnitEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IBaseUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IBaseUnitEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IAreaEffectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAreaEffectEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IMapObjectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMapObjectEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IItemEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IItemEntity>
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T, TInvoker>(TInvoker entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<TInvoker> where TInvoker : IEntity
	{
		Shared.RaiseEvent(entity, action, isCheckRuntime);
	}
}
