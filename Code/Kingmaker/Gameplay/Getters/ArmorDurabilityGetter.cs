using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
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
		return Type switch
		{
			ValueType.Current => base.CurrentEntity.GetArmorOptional()?.DurabilityLeft ?? 0, 
			ValueType.Max => base.CurrentEntity.GetArmorOptional()?.DurabilityValue ?? 0, 
			ValueType.Percent => (int)((base.CurrentEntity.GetArmorOptional()?.DurabilityLeftFraction ?? 0f) * 100f), 
			ValueType.ArmourItem => (base.CurrentEntity.GetBodyOptional()?.Armor.MaybeArmor?.ArmorDurability).GetValueOrDefault(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{Type} value of armor durability";
	}
}
