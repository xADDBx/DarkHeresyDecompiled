using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("d9cd692d15b98a44b9f67c147ad5b412")]
public class TutorialTriggerItemChargesSpent : TutorialTrigger, IItemChargesHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	private bool CanTrigger => m_Item.Get().SpendCharges;

	public void HandleItemChargeSpent(ItemEntity item)
	{
		if (!CanTrigger)
		{
			throw new Exception("TutorialTriggerItemChargesSpent: Cannot trigger if Item can't spend charges");
		}
		if (m_Item.Get() == item.Blueprint)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
			});
		}
	}
}
