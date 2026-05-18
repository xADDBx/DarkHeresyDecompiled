using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("07756321110b4928b14d4f0bb31478d5")]
public class CheckIsHeavyWeaponGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if weapon of ability is heavy";
	}

	protected override bool GetBaseValue()
	{
		ItemEntityWeapon? abilityWeapon = EvalContext.Current.AbilityWeapon;
		if (abilityWeapon == null)
		{
			return false;
		}
		return abilityWeapon.Blueprint.Heaviness == WeaponHeaviness.Heavy;
	}
}
