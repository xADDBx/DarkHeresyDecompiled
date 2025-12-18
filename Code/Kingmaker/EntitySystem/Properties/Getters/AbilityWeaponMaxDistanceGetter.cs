using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1d97120646e407b4d83d019aa7138928")]
public class AbilityWeaponMaxDistanceGetter : IntPropertyGetter, PropertyContextAccessor.IAbilityWeapon, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	protected override int GetBaseValue()
	{
		return this.GetAbilityWeapon().Blueprint.WarhammerMaxDistance;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Max Distance";
	}
}
