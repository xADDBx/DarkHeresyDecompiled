using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d01e99869de9449883f38942466906cb")]
public class CheckCanUseAbilityOnTargetGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		AbilityData ability = EvalContext.Current.Ability;
		if (ability == null)
		{
			return false;
		}
		MechanicEntity entityByType = EvalContext.Current.GetEntityByType(Target);
		return ability.CanTarget(entityByType);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if can use ability on " + Target.Colorized();
	}
}
