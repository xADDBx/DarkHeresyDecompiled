using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Int;

[Serializable]
[TypeId("04ca60db55734722a8ffc7031ad1d0c0")]
public sealed class AbilityWeaponSetGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Weapon set of ability";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = EvalContext.Current.AbilityWeapon;
		PartUnitBody partUnitBody = abilityWeapon?.Wielder?.GetBodyOptional();
		if (partUnitBody == null)
		{
			return 0;
		}
		return (partUnitBody.GetHandsEquipmentSetIndex(abilityWeapon) + 1).GetValueOrDefault();
	}
}
