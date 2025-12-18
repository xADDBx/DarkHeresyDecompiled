using System;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("7b2bdafb4c5c468382f7ab8bf73a2c45")]
public abstract class ItemChargeSpentTrigger : MechanicEntityFactComponentDelegate
{
	public enum ItemFilterType
	{
		SingleItem,
		ItemList,
		ItemTag
	}

	public enum TriggerType
	{
		LastChargeSpent,
		EveryChargeSpent
	}

	[InfoBox("CurrentTarget is user of item")]
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public TriggerType Trigger;

	public ItemFilterType Filter;

	[ShowIf("IsSingleItem")]
	public BpRef<BlueprintItemEquipment> SingleItem = new BpRef<BlueprintItemEquipment>();

	[ShowIf("IsItemList")]
	public BpRef<BlueprintItemEquipment>[] ItemList = new BpRef<BlueprintItemEquipment>[0];

	[ShowIf("IsItemTag")]
	public ItemTag TagFilter;

	public ActionList Actions = new ActionList();

	private bool IsSingleItem => Filter == ItemFilterType.SingleItem;

	private bool IsItemList => Filter == ItemFilterType.ItemList;

	private bool IsItemTag => Filter == ItemFilterType.ItemTag;

	protected void TryTrigger(ItemEntity item, MechanicEntity user)
	{
		if (!IsSuitable(item, user))
		{
			return;
		}
		using (ContextData<ItemEntity.ContextData>.Request().Setup(item))
		{
			Actions.Run();
		}
	}

	private bool IsSuitable(ItemEntity item, MechanicEntity user)
	{
		bool flag = Filter switch
		{
			ItemFilterType.SingleItem => SingleItem.MaybeBlueprint == item.Blueprint, 
			ItemFilterType.ItemList => ItemList.Dereference().Contains(item.Blueprint), 
			ItemFilterType.ItemTag => TagFilter == item.Blueprint.Tag, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (flag)
		{
			flag = Trigger switch
			{
				TriggerType.LastChargeSpent => item.Charges < 1 && item.Count < 2, 
				TriggerType.EveryChargeSpent => true, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (flag)
		{
			return Restrictions.IsPassed(base.Context, null, user);
		}
		return false;
	}
}
