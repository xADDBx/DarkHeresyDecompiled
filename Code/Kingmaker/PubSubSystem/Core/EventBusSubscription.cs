using System;
using JetBrains.Annotations;

namespace Kingmaker.PubSubSystem.Core;

public readonly struct EventBusSubscription : IDisposable
{
	[NotNull]
	private readonly EventBusInstance m_EventBus;

	private readonly object m_Subscriber;

	public EventBusSubscription([NotNull] EventBusInstance eventBus, object subscriber)
	{
		m_EventBus = eventBus;
		m_Subscriber = subscriber;
	}

	public void Dispose()
	{
		m_EventBus.Unsubscribe(m_Subscriber);
	}
}
