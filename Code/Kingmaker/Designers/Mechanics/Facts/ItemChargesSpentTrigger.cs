using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("8a1c60eb02e9ae14bb662f79c0bf63d9")]
public class ItemChargesSpentTrigger : EntityFactComponentDelegate, IItemChargesHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	[SerializeField]
	private ActionList m_Actions;

	[SerializeField]
	private bool m_Once;

	private bool CanTrigger => m_Item.Get().SpendCharges;

	public void HandleItemChargeSpent(ItemEntity item)
	{
		if (!CanTrigger)
		{
			throw new Exception("ItemChargesSpent: Cannot trigger if Item can't spend charges");
		}
		if ((!m_Once || base.ExecutesCount <= 0) && m_Item.Get() == item.Blueprint)
		{
			m_Actions.Run();
			base.ExecutesCount++;
		}
	}
}
