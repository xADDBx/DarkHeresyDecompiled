using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Components;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Items.Components;

[Serializable]
[ComponentName("Items/ItemChargeSpentTriggerInitiator")]
[TypeId("22da5906931348f0a205668512777f58")]
public sealed class ItemChargeSpentTriggerInitiator : ItemChargeSpentTrigger, IItemChargesHandler<EntitySubscriber>, IItemChargesHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IItemChargesHandler, EntitySubscriber>
{
	void IItemChargesHandler.HandleItemChargeSpent(ItemEntity item)
	{
		TryTrigger(item, base.Owner);
	}
}
