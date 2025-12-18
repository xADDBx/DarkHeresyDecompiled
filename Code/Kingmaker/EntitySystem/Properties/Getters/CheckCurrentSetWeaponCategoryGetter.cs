using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b585164c2ef84684da8d07d537c362fc")]
public class CheckCurrentSetWeaponCategoryGetter : BoolPropertyGetter
{
	public enum Hand
	{
		Primary,
		Secondary
	}

	public WeaponCategory[] Categories;

	public Hand hand;

	protected override bool GetBaseValue()
	{
		HandsEquipmentSet handsEquipmentSet = base.CurrentEntity.GetBodyOptional()?.CurrentHandsEquipmentSet;
		if (handsEquipmentSet == null)
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = hand switch
		{
			Hand.Primary => handsEquipmentSet.PrimaryHand.MaybeWeapon, 
			Hand.Secondary => handsEquipmentSet.SecondaryHand.MaybeWeapon, 
			_ => null, 
		};
		if (itemEntityWeapon == null)
		{
			return false;
		}
		return Categories.IndexOf(itemEntityWeapon.Blueprint.Category) >= 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Weapon in {hand} hand category";
	}
}
