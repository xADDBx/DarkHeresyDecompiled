using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a88aca1f99c04b5abe76a4f8ff634e37")]
public class CheckAbilityIsAoEGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool IncludeAreaEffects;

	public bool DoNotIncludeScatter;

	protected override bool GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if (ability == null)
		{
			return false;
		}
		if (ability.Blueprint.PatternSettings == null || (DoNotIncludeScatter && ability.Blueprint.PatternSettings is BurstPattern))
		{
			if (IncludeAreaEffects)
			{
				return ability.IsAoe;
			}
			return false;
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability has pattern";
	}
}
