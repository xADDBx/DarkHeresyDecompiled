using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters.Damage;

[Serializable]
[TypeId("4300ddd95b9c454c991389bd86bc6929")]
public class CheckAbilityDamageGetter : CheckDamageGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalRule, PropertyContextAccessor.IOptional
{
	protected override bool Check(out DamageType type, out IntermediateDamage data, out RulebookEvent rule)
	{
		data = this.GetAbility().GetWeaponStats().BaseDamage;
		type = data?.Type ?? DamageType.None;
		rule = this.GetRule();
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return base.GetInnerCaption(useLineBreaks: false) + " (Ability)";
	}
}
