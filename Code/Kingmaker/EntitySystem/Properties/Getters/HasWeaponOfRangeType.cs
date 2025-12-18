using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1773ce4423b542d2932f6e101806c26e")]
public class HasWeaponOfRangeType : BoolPropertyGetter
{
	public bool Melee;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!Melee)
		{
			return "Any weapon of " + FormulaTargetScope.Current + " is Ranged";
		}
		return "Any weapon of " + FormulaTargetScope.Current + " is Melee";
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		ItemEntityWeapon maybeWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		int num;
		if (maybeWeapon == null || !maybeWeapon.Blueprint.IsMelee || !Melee)
		{
			ItemEntityWeapon maybeWeapon2 = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
			num = ((maybeWeapon2 != null && maybeWeapon2.Blueprint.IsRanged && !Melee) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		ItemEntityWeapon maybeWeapon3 = baseUnitEntity.Body.SecondaryHand.MaybeWeapon;
		int num2;
		if (maybeWeapon3 == null || !maybeWeapon3.Blueprint.IsMelee || !Melee)
		{
			ItemEntityWeapon maybeWeapon4 = baseUnitEntity.Body.SecondaryHand.MaybeWeapon;
			num2 = ((maybeWeapon4 != null && maybeWeapon4.Blueprint.IsRanged && !Melee) ? 1 : 0);
		}
		else
		{
			num2 = 1;
		}
		bool flag = (byte)num2 != 0;
		bool flag2 = (Melee && baseUnitEntity.Body.AdditionalLimbs.Any((WeaponSlot p) => p.MaybeWeapon?.Blueprint.IsMelee ?? false)) || (!Melee && baseUnitEntity.Body.AdditionalLimbs.Any((WeaponSlot p) => p.MaybeWeapon?.Blueprint.IsRanged ?? false));
		return (byte)((uint)num | (flag ? 1u : 0u) | (flag2 ? 1u : 0u)) != 0;
	}
}
