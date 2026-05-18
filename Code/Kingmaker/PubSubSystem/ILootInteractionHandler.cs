using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface ILootInteractionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback);

	void HandleZoneLootInteraction(AreaTransitionPart areaTransition);
}
