using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1d97120646e407b4d83d019aa7138928")]
public class AbilityWeaponMaxDistanceGetter : IntPropertyGetter, PropertyContextAccessor.IAbilityWeapon, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return EvalContext.Current.AbilityWeapon.Blueprint.WarhammerMaxDistance;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Max Distance";
	}
}
