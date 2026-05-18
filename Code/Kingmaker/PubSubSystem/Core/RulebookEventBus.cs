using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookEventBus
{
	[NotNull]
	public static RulebookSubscriptionManager<IGlobalRulebookSubscriber> GlobalRulebookSubscribers => EventBus.Shared.GlobalRulebookSubscribers;

	[NotNull]
	public static PooledDictionary<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>> TargetRulebookSubscribers => EventBus.Shared.TargetRulebookSubscribers;

	[NotNull]
	public static PooledDictionary<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>> InitiatorRulebookSubscribers => EventBus.Shared.InitiatorRulebookSubscribers;

	public static void OnEventAboutToTrigger<T>(T evt) where T : IRulebookEvent
	{
		EventBus.Shared.OnEventAboutToTrigger(evt);
	}

	public static void OnEventDidTrigger<T>(T evt) where T : IRulebookEvent
	{
		EventBus.Shared.OnEventDidTrigger(evt);
	}
}
