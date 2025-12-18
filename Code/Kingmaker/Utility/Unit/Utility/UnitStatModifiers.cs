using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Utility.Unit.Utility;

[Serializable]
public sealed class UnitStatModifiers
{
	[Serializable]
	public sealed class AttributeCategoryIncrease
	{
		public AttributeType Attribute;

		[Range(1f, 4f)]
		[SerializeField]
		private int _categoryIncrease;

		public int CategoryIncrease => Math.Max(1, _categoryIncrease);
	}

	public AttributeCategoryIncrease[] Attributes = new AttributeCategoryIncrease[0];

	[InfoBox("Если true - юнит будет получать броню от предметов, если false - юнит получит рассчитанную для его настроек броню. Для компаньонов всегда считается true.")]
	public bool UseArmorOfEquipment;

	[InfoBox("Процентный модификатор для HP юнитов (+/-X%)")]
	[Range(-100f, 200f)]
	public int HitPoints;

	[InfoBox("Процентный модификатор для брони юнитов (+/-X%)")]
	[Range(-100f, 200f)]
	[HideIf("UseArmorOfEquipment")]
	public int ArmorDurability;

	public int GetAttributeCategoryIncrease(AttributeType attribute)
	{
		return Array.Find(Attributes, (AttributeCategoryIncrease i) => i.Attribute == attribute)?.CategoryIncrease ?? 0;
	}
}
