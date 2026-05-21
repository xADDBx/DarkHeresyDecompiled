using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1a41043e24de3b44d9579d03606df6c8")]
public class CheckAbilityWeaponFactionGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public ItemFaction Faction;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability weapon faction == {Faction}";
	}

	protected override bool GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = EvalContext.Current.AbilityWeapon;
		if (abilityWeapon != null)
		{
			return abilityWeapon.EffectiveFaction == Faction;
		}
		return false;
	}
}
