using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("4549ac0b64e944b695fa4f8dc8e3c8d8")]
public sealed class ArmorDurabilityGetter : IntPropertyGetter
{
	public enum ValueType
	{
		Current,
		Max,
		Percent,
		ArmourItem
	}

	public ValueType Type;

	protected override int GetBaseValue()
	{
		switch (Type)
		{
		case ValueType.Current:
			return base.CurrentEntity.GetArmorOptional()?.DurabilityLeft ?? 0;
		case ValueType.Max:
		{
			ModifiableValue modifiableValue = base.CurrentEntity.GetArmorOptional()?.Durability;
			return (modifiableValue != null) ? ((int)modifiableValue) : 0;
		}
		case ValueType.Percent:
			return (int)((base.CurrentEntity.GetArmorOptional()?.DurabilityLeftFraction ?? 0f) * 100f);
		case ValueType.ArmourItem:
			return (base.CurrentEntity.GetBodyOptional()?.Armor.MaybeArmor?.Blueprint.ArmorDurability).GetValueOrDefault();
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{Type} value of armor durability";
	}
}
