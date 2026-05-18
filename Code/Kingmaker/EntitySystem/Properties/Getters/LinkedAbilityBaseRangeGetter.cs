using System;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c7db2c0302e5a744cb4582e724ac3a9f")]
public class LinkedAbilityBaseRangeGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbilityWeapon
{
	protected override int GetBaseValue()
	{
		AbilityData ability = EvalContext.Current.Ability;
		if (ability == null)
		{
			return 0;
		}
		if (!ability.Blueprint.AllModifiers.Contains(base.Owner as BlueprintAbilityModifier))
		{
			return 0;
		}
		return ability.Blueprint.Range switch
		{
			AbilityRange.Custom => ability.Blueprint.CustomRange, 
			AbilityRange.Weapon => EvalContext.Current.AbilityWeapon?.Blueprint.WarhammerMaxDistance ?? 0, 
			_ => 0, 
		};
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Linked ability base range";
	}
}
