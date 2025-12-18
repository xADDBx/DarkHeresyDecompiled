using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("2d576eefddb840c5bc0c68939c8f6d89")]
public class TutorialTriggerPlayerGotUsableItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber
{
	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if ((collection == null || collection.IsPlayerInventory) && item.Blueprint is BlueprintItemEquipmentUsable)
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
