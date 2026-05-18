using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Items.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("306896bb4db199543b4cacdd5479d64c")]
public class AbilityWeaponPowerLevelGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return (int)(this.GetAbilityWeapon()?.PowerLevel ?? ItemPowerLevel.Undefined);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Weapon effective power level (0..8)";
	}
}
