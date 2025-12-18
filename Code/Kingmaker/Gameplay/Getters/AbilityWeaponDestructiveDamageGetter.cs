using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("2217331111f245d192edf4a19f442563")]
public sealed class AbilityWeaponDestructiveDamageGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability weapon Destructive damage";
	}

	protected override int GetBaseValue()
	{
		return this.GetAbilityWeapon()?.Blueprint.DestructiveDamage ?? 0;
	}
}
