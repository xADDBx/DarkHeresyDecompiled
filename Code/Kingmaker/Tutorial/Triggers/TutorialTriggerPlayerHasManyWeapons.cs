using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("cfa5ce3b77ef4a67a153c9d3352c2527")]
public class TutorialTriggerPlayerHasManyWeapons : TutorialTrigger, IItemsCollectionHandler, ISubscriber
{
	[SerializeField]
	private int m_Value = 4;

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if ((collection == null || collection.IsPlayerInventory) && item.Blueprint is BlueprintItemWeapon && GameHelper.GetPlayerCharacter().Inventory.OfType<ItemEntityWeapon>().Count() > m_Value)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
				context.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
	}
}
