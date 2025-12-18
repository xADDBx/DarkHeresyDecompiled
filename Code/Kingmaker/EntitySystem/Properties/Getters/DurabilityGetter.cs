using System;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("74d040238d694bbdae53a5f30c5de8aa")]
public class DurabilityGetter : IntPropertyGetter
{
	[SerializeField]
	private bool IsArmorPenetrated;

	[SerializeField]
	[HideIf("IsArmorPenetrated")]
	private DurabilityHpValueType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (IsArmorPenetrated)
		{
			return "Is the armor penetrated";
		}
		return Type switch
		{
			DurabilityHpValueType.Current => "Armor durability of " + FormulaTargetScope.Current, 
			DurabilityHpValueType.CurrentPercent => "Armor durability percent of " + FormulaTargetScope.Current, 
			DurabilityHpValueType.Max => "Maximum armor durability of " + FormulaTargetScope.Current, 
			DurabilityHpValueType.FromItems => "Armor durability of equipment on " + FormulaTargetScope.Current, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override int GetBaseValue()
	{
		PartArmor armorOptional = base.CurrentEntity.GetArmorOptional();
		if (armorOptional == null)
		{
			return 0;
		}
		if (IsArmorPenetrated)
		{
			if (armorOptional.DurabilityLeft > 0)
			{
				return 0;
			}
			return 1;
		}
		return Type switch
		{
			DurabilityHpValueType.Current => armorOptional.DurabilityLeft, 
			DurabilityHpValueType.CurrentPercent => (int)Math.Floor((float)armorOptional.DurabilityLeft * 100f / (float)(int)armorOptional.Durability), 
			DurabilityHpValueType.Max => armorOptional.Durability, 
			DurabilityHpValueType.FromItems => GetDurabilityFromItems(base.CurrentEntity), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private int GetDurabilityFromItems(MechanicEntity entity)
	{
		return (entity.GetBodyOptional()?.Items?.Sum((Func<ItemEntity, int>)GetDurabilityFromItem)).GetValueOrDefault();
	}

	private int GetDurabilityFromItem(ItemEntity itemEntity)
	{
		int num = 0;
		if (itemEntity is ItemEntityArmor itemEntityArmor)
		{
			num += itemEntityArmor.Blueprint.ArmorDurability;
		}
		return num + itemEntity.Blueprint.GetComponents<AddFactToEquipmentWielder>().Sum((AddFactToEquipmentWielder addFact) => (from addStat in addFact.Fact.GetComponents<AddStatBonus>()
			where addStat.Stat == StatType.ArmorDurability
			select addStat).Sum((AddStatBonus addStat) => addStat.Value));
	}
}
