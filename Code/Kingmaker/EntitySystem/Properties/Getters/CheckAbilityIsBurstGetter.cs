using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("402f67d675fd4705bed57b358df0798d")]
public class CheckAbilityIsBurstGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		AbilityData ability = EvalContext.Current.Ability;
		if (ability == null)
		{
			return false;
		}
		return ability.Blueprint.GetComponent<AbilityAttackDelivery>()?.IsBurst ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability is burst shot";
	}
}
