using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ISubscriptionProxy
{
	[CanBeNull]
	ISubscriber GetSubscriber();

	EvalContext.StackFrameHandle RequestEventContext();

	[CanBeNull]
	IEntity GetSubscribingEntity();
}
