using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("39a40d5018244bd498cbbe39fb22c9e3")]
public class CheckHasTwoWeaponsOfClassificationGetter : BoolPropertyGetter
{
	public WeaponClassification Classification;

	protected override bool GetBaseValue()
	{
		if (base.CurrentEntity.GetBodyOptional()?.PrimaryHand?.MaybeWeapon?.Blueprint?.Classification == Classification)
		{
			return base.CurrentEntity.GetBodyOptional()?.SecondaryHand?.MaybeWeapon?.Blueprint?.Classification == Classification;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Checks if current entity has two weapons with selected classification";
	}
}
