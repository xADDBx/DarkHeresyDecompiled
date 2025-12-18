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
[ComponentName("Items/ItemChargeSpentTriggerGlobal")]
[TypeId("71971fe40d86423f82430833b12f1d05")]
public sealed class ItemChargeSpentTriggerGlobal : ItemChargeSpentTrigger, IItemChargesHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	void IItemChargesHandler.HandleItemChargeSpent(ItemEntity item)
	{
		TryTrigger(item, EventInvokerExtensions.MechanicEntity);
	}
}
