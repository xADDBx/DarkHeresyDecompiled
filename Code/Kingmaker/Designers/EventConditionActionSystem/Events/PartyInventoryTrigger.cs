using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Items/PartyInventoryTrigger (ItemTrigger)")]
[AllowMultipleComponents]
[TypeId("2692cd97dff261b40b530d7b25e425cd")]
public class PartyInventoryTrigger : EntityFactComponentDelegate, IItemsCollectionHandler, ISubscriber
{
	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public ActionList OnAddActions;

	public ActionList OnRemoveActions;

	public BlueprintItem Item => m_Item?.Get();

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint != Item)
		{
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.Inventory.Collection == collection)
			{
				OnAddActions.Run();
				break;
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint != Item)
		{
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.Inventory.Collection == collection)
			{
				OnRemoveActions.Run();
				break;
			}
		}
	}
}
