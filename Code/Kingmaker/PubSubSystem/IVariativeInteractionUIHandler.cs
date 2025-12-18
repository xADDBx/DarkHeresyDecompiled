using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVariativeInteractionUIHandler : ISubscriber
{
	void HandleInteractionRequest(MechanicEntity mechanicEntity, IEnumerable<InteractionActorWithConditions> actors = null);

	void HandleInteractionRequest(InteractionVariativePart interactionPart);
}
