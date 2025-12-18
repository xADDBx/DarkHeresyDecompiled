using System;
using System.Collections.Generic;
using System.Text;
using Framework.Utility.DotNetExtensions;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[ComponentName("Equipment/CostOfEquippedItemsGetter")]
[TypeId("cb497cf639bf4705b07ca3529fa5bc8c")]
public sealed class CostOfEquippedItemsGetter : IntPropertyGetter
{
	public bool ExcludeWeapons;

	public bool ExcludeArmor;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		StringBuilder value;
		using (StringBuilderPool.Get(out value))
		{
			value.Append("Cost of all equipped items");
			if (ExcludeWeapons || ExcludeArmor)
			{
				value.Append(" (excluding ");
				if (ExcludeWeapons)
				{
					value.Append("weapons");
				}
				if (ExcludeArmor)
				{
					value.Append(ExcludeWeapons ? " and armor" : "armor");
				}
				value.Append(")");
			}
			return value.ToString();
		}
	}

	protected override int GetBaseValue()
	{
		List<ItemSlot> list = base.CurrentEntity.GetBodyOptional()?.EquipmentSlots;
		if (list == null)
		{
			return 0;
		}
		int num = 0;
		foreach (ItemSlot item in list)
		{
			ItemEntity maybeItem = item.MaybeItem;
			if (maybeItem != null && (!ExcludeWeapons || !(maybeItem is ItemEntityWeapon)) && (!ExcludeArmor || !(maybeItem is ItemEntityArmor)))
			{
				num += maybeItem.Blueprint.Cost;
			}
		}
		return num;
	}
}
