using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Getters;

[Serializable]
[TypeId("5312420be6af46518c85803a2d51c12a")]
public class CheckAmountOfWeaponsInHandsGetter : BoolPropertyGetter
{
	public enum Weapons
	{
		None,
		One,
		Two
	}

	public Weapons WeaponsAmount;

	protected override bool GetBaseValue()
	{
		int num = 0;
		num += (base.CurrentEntity.GetBodyOptional()?.PrimaryHand?.MaybeWeapon?.Blueprint ? 1 : 0);
		num += (base.CurrentEntity.GetBodyOptional()?.SecondaryHand?.MaybeWeapon?.Blueprint ? 1 : 0);
		return WeaponsAmount switch
		{
			Weapons.None => num == 0, 
			Weapons.One => num == 1, 
			Weapons.Two => num == 2, 
			_ => false, 
		};
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + WeaponsAmount.ToString() + " Weapons equipped in current set?";
	}
}
