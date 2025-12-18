using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("c38209541b9846e3ab8bfdc565269ddf")]
public class TutorialTriggerPlayerGotSoulMarkRestrictedItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber
{
	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && !collection.IsPlayerInventory)
		{
			return;
		}
		IEnumerable<EquipmentRestrictionHasFacts> enumerable = item.Blueprint.ComponentsArray.OfType<EquipmentRestrictionHasFacts>();
		if (enumerable.Empty())
		{
			return;
		}
		foreach (EquipmentRestrictionHasFacts item2 in enumerable)
		{
			if (!item2.Facts.OfType<BlueprintAlignmentMark>().Empty())
			{
				TryToTrigger(null, delegate(TutorialContext context)
				{
					context.SourceItem = item;
					context.SourceUnit = GameHelper.GetPlayerCharacter();
				});
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
	}
}
