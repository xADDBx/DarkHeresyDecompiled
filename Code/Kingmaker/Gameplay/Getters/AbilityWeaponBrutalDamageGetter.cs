using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("9fbbe43b1cd34b65a67ca727595883ba")]
[ComponentName("Weapon/AbilityWeaponBrutalDamageGetter")]
public sealed class AbilityWeaponBrutalDamageGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability weapon Brutal damage";
	}

	protected override int GetBaseValue()
	{
		return this.GetAbilityWeapon()?.Blueprint.BrutalDamage ?? 0;
	}
}
