using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("3f60e4821ce44e3478fcfb29af723474")]
public class TutorialTriggerItemInserted : TutorialTrigger, IInsertItemHandler, ISubscriber
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	public void HandleInsertItem(ItemSlot slot)
	{
		if (m_Item.GetBlueprint() == slot.Item.Blueprint)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = slot.Item;
				context.SourceUnit = slot.Item.Owner as BaseUnitEntity;
			});
		}
	}
}
