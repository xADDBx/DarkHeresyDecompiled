using System;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookEvent
{
	bool IsTriggered { get; }

	IMechanicEntity Target { get; }

	IMechanicEntity Initiator { get; }

	IMechanicEntity Self { get; }

	bool IsGameLogDisabled { get; }

	Type RootType { get; }

	void OnDidTrigger();

	void OnTrigger(RulebookEventContext rulebookEventContext);
}
