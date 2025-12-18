using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.PubSubSystem;

public interface IInteractionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void OnInteract(AbstractInteractionPart interaction);

	void OnInteractionRestricted(AbstractInteractionPart interaction);
}
