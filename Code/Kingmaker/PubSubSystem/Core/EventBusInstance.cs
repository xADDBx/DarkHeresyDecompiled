using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public class EventBusInstance : IEntityEventBus
{
	[NotNull]
	public readonly SubscriptionManager<ISubscriber> GlobalSubscribers = new SubscriptionManager<ISubscriber>();

	[NotNull]
	public readonly PooledDictionary<IEntity, SubscriptionManager<ISubscriber>> EntitySubscribers = new PooledDictionary<IEntity, SubscriptionManager<ISubscriber>>();

	[NotNull]
	public readonly RulebookSubscriptionManager<IGlobalRulebookSubscriber> GlobalRulebookSubscribers = new RulebookSubscriptionManager<IGlobalRulebookSubscriber>();

	[NotNull]
	public readonly PooledDictionary<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>> TargetRulebookSubscribers = new PooledDictionary<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>>();

	[NotNull]
	public readonly PooledDictionary<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>> InitiatorRulebookSubscribers = new PooledDictionary<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>>();

	[NotNull]
	public readonly RulebookEventHooksManager RulebookHooks = new RulebookEventHooksManager();

	public EventBusSubscription Subscribe([CanBeNull] object subscriber)
	{
		SubscribeEventBus(subscriber);
		SubscribeRulebook(subscriber);
		return new EventBusSubscription(this, subscriber);
	}

	public void Unsubscribe([CanBeNull] object subscriber)
	{
		UnsubscribeEventBus(subscriber);
		UnsubscribeRulebook(subscriber);
	}

	public void ClearEntitySubscriptions([NotNull] IEntity entity)
	{
		EntitySubscribers.Remove(entity);
		ClearRulebookEntitySubscriptions(entity);
	}

	public bool IsGloballySubscribed(object subscriber)
	{
		if (!(subscriber is ISubscriber subscriber2))
		{
			return false;
		}
		return GlobalSubscribers.Contains(subscriber2);
	}

	public void LogActiveSubscribers()
	{
		PFLog.EventSystemDebug.Log("====> GLOBAL");
		GlobalSubscribers.LogActiveSubscribers();
		IEntity key;
		foreach (KeyValuePair<IEntity, SubscriptionManager<ISubscriber>> entitySubscriber in EntitySubscribers)
		{
			entitySubscriber.Deconstruct(out key, out var value);
			IEntity arg = key;
			SubscriptionManager<ISubscriber> subscriptionManager = value;
			PFLog.EventSystemDebug.Log($"====> {arg}");
			subscriptionManager.LogActiveSubscribers();
		}
		PFLog.EventSystemDebug.Log("====> RULEBOOK GLOBAL");
		GlobalRulebookSubscribers.LogActiveSubscribers();
		foreach (KeyValuePair<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>> targetRulebookSubscriber in TargetRulebookSubscribers)
		{
			targetRulebookSubscriber.Deconstruct(out key, out var value2);
			IEntity arg2 = key;
			RulebookSubscriptionManager<ITargetRulebookSubscriber> rulebookSubscriptionManager = value2;
			PFLog.EventSystemDebug.Log($"====> {arg2}");
			rulebookSubscriptionManager.LogActiveSubscribers();
		}
		foreach (KeyValuePair<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>> initiatorRulebookSubscriber in InitiatorRulebookSubscribers)
		{
			initiatorRulebookSubscriber.Deconstruct(out key, out var value3);
			IEntity arg3 = key;
			RulebookSubscriptionManager<IInitiatorRulebookSubscriber> rulebookSubscriptionManager2 = value3;
			PFLog.EventSystemDebug.Log($"====> {arg3}");
			rulebookSubscriptionManager2.LogActiveSubscribers();
		}
	}

	public void RaiseEvent<T>(Action<T> action, bool isCheckRuntime = true) where T : ISubscriber
	{
		try
		{
			GlobalSubscribers.RaiseEvent(action, isCheckRuntime);
		}
		finally
		{
		}
	}

	public void RaiseEvent<T>(IEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IEntity>
	{
		this.RaiseEvent<T, IEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IMechanicEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMechanicEntity>
	{
		this.RaiseEvent<T, IMechanicEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IAbstractUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAbstractUnitEntity>
	{
		this.RaiseEvent<T, IAbstractUnitEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IBaseUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IBaseUnitEntity>
	{
		this.RaiseEvent<T, IBaseUnitEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IAreaEffectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAreaEffectEntity>
	{
		this.RaiseEvent<T, IAreaEffectEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IMapObjectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMapObjectEntity>
	{
		this.RaiseEvent<T, IMapObjectEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T>(IItemEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IItemEntity>
	{
		this.RaiseEvent<T, IItemEntity>(entity, action, isCheckRuntime);
	}

	public void RaiseEvent<T, TInvoker>(TInvoker entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<TInvoker> where TInvoker : IEntity
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		try
		{
			using (ContextData<EventInvoker>.Request().Setup(entity))
			{
				RaiseEvent(action, isCheckRuntime);
			}
			EntitySubscribers.Get(entity)?.RaiseEvent(action);
		}
		finally
		{
		}
	}

	public void OnEventAboutToTrigger<T>(T evt) where T : IRulebookEvent
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			RulebookHooks.OnBeforeEventAboutToTrigger(evt);
			GlobalRulebookSubscribers.OnEventAboutToTrigger(evt);
			IMechanicEntity target = evt.Target;
			if (target != null)
			{
				TargetRulebookSubscribers.Get(target)?.OnEventAboutToTrigger(evt);
			}
			InitiatorRulebookSubscribers.Get(evt.Initiator)?.OnEventAboutToTrigger(evt);
		}
	}

	public void OnEventDidTrigger<T>(T evt) where T : IRulebookEvent
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			GlobalRulebookSubscribers.OnEventDidTrigger(evt);
			IMechanicEntity target = evt.Target;
			if (target != null)
			{
				TargetRulebookSubscribers.Get(target)?.OnEventDidTrigger(evt);
			}
			InitiatorRulebookSubscribers.Get(evt.Initiator)?.OnEventDidTrigger(evt);
			RulebookHooks.OnAfterEventDidTrigger(evt);
		}
	}

	private void SubscribeEventBus([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			SubscribeEventBus(subscriptionProxy.GetSubscriber(), subscriptionProxy);
		}
		SubscribeEventBus(subscriber as ISubscriber, null);
	}

	private void UnsubscribeEventBus([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			UnsubscribeEventBus(subscriptionProxy.GetSubscriber(), subscriptionProxy);
		}
		UnsubscribeEventBus(subscriber as ISubscriber, null);
	}

	private void SubscribeEventBus([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		SubscribeGlobal(subscriber, proxy);
		SubscribeBinded(subscriber, proxy);
	}

	private void UnsubscribeEventBus([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		UnsubscribeGlobal(subscriber, proxy);
		UnsubscribeBinded(subscriber, proxy);
	}

	private void SubscribeGlobal([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalSubscribers.Subscribe<EventTagNone>(subscriber, proxy);
		}
	}

	private void UnsubscribeGlobal([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalSubscribers.Unsubscribe<EventTagNone>(subscriber, proxy);
		}
	}

	private void SubscribeBinded([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? (subscriber as IEntitySubscriber)?.GetSubscribingEntity();
			if (entity != null)
			{
				EntitySubscribers.Sure(entity).Subscribe<EntitySubscriber>(subscriber, proxy);
			}
		}
	}

	private void UnsubscribeBinded([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? (subscriber as IEntitySubscriber)?.GetSubscribingEntity();
		if (entity == null)
		{
			return;
		}
		SubscriptionManager<ISubscriber> subscriptionManager = EntitySubscribers.Get(entity);
		if (subscriptionManager != null)
		{
			subscriptionManager.Unsubscribe<EntitySubscriber>(subscriber, proxy);
			if (subscriptionManager.Empty())
			{
				EntitySubscribers.Remove(entity);
			}
		}
	}

	private void SubscribeRulebook([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			SubscribeGlobalRulebook(subscriptionProxy.GetSubscriber() as IGlobalRulebookSubscriber, subscriptionProxy);
			SubscribeTargetRulebook(subscriptionProxy.GetSubscriber() as ITargetRulebookSubscriber, subscriptionProxy);
			SubscribeInitiatorRulebook(subscriptionProxy.GetSubscriber() as IInitiatorRulebookSubscriber, subscriptionProxy);
		}
		SubscribeGlobalRulebook(subscriber as IGlobalRulebookSubscriber, null);
		SubscribeTargetRulebook(subscriber as ITargetRulebookSubscriber, null);
		SubscribeInitiatorRulebook(subscriber as IInitiatorRulebookSubscriber, null);
		RulebookHooks.Register(subscriber);
	}

	private void UnsubscribeRulebook([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			UnsubscribeGlobalRulebook(subscriptionProxy.GetSubscriber() as IGlobalRulebookSubscriber, subscriptionProxy);
			UnsubscribeTargetRulebook(subscriptionProxy.GetSubscriber() as ITargetRulebookSubscriber, subscriptionProxy);
			UnsubscribeInitiatorRulebook(subscriptionProxy.GetSubscriber() as IInitiatorRulebookSubscriber, subscriptionProxy);
		}
		UnsubscribeGlobalRulebook(subscriber as IGlobalRulebookSubscriber, null);
		UnsubscribeTargetRulebook(subscriber as ITargetRulebookSubscriber, null);
		UnsubscribeInitiatorRulebook(subscriber as IInitiatorRulebookSubscriber, null);
		RulebookHooks.Unregister(subscriber);
	}

	private void ClearRulebookEntitySubscriptions([NotNull] IEntity unit)
	{
		TargetRulebookSubscribers.Remove(unit);
		InitiatorRulebookSubscribers.Remove(unit);
	}

	private void SubscribeGlobalRulebook([CanBeNull] IGlobalRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalRulebookSubscribers.Subscribe(subscriber, proxy);
		}
	}

	private void UnsubscribeGlobalRulebook([CanBeNull] IGlobalRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalRulebookSubscribers.Unsubscribe(subscriber, proxy);
		}
	}

	private void SubscribeTargetRulebook([CanBeNull] ITargetRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
			if (entity == null)
			{
				PFLog.Default.Error("Could not subscribe {0}, it didnt provide a unit", subscriber);
			}
			else
			{
				TargetRulebookSubscribers.Sure(entity).Subscribe(subscriber, proxy);
			}
		}
	}

	private void SubscribeInitiatorRulebook([CanBeNull] IInitiatorRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
			if (entity == null)
			{
				PFLog.Default.Error("Could not subscribe {0}, it didnt provide a unit", subscriber);
			}
			else
			{
				InitiatorRulebookSubscribers.Sure(entity).Subscribe(subscriber, proxy);
			}
		}
	}

	private void UnsubscribeTargetRulebook([CanBeNull] ITargetRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
		if (entity == null)
		{
			PFLog.Default.Error("Could not unsubscribe {0}, it didnt provide a unit", subscriber);
			return;
		}
		RulebookSubscriptionManager<ITargetRulebookSubscriber> rulebookSubscriptionManager = TargetRulebookSubscribers.Get(entity);
		if (rulebookSubscriptionManager != null)
		{
			rulebookSubscriptionManager.Unsubscribe(subscriber, proxy);
			if (rulebookSubscriptionManager.Empty())
			{
				TargetRulebookSubscribers.Remove(entity);
			}
		}
	}

	private void UnsubscribeInitiatorRulebook([CanBeNull] IInitiatorRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
		if (entity == null)
		{
			PFLog.Default.Error("Could not unsubscribe {0}, it didnt provide a unit", subscriber);
			return;
		}
		RulebookSubscriptionManager<IInitiatorRulebookSubscriber> rulebookSubscriptionManager = InitiatorRulebookSubscribers.Get(entity);
		if (rulebookSubscriptionManager != null)
		{
			rulebookSubscriptionManager.Unsubscribe(subscriber, proxy);
			if (rulebookSubscriptionManager.Empty())
			{
				InitiatorRulebookSubscribers.Remove(entity);
			}
		}
	}
}
