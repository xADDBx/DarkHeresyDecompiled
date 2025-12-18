using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Items.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("716c2db2bbaccdb4bae2feb33b4cbf28")]
public class CheckWeaponProgressionTypeGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponProgressionType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if weapon has selected progression type";
	}

	protected override bool GetBaseValue()
	{
		return this.GetAbilityWeapon()?.Blueprint.ProgressionType == Type;
	}
}
