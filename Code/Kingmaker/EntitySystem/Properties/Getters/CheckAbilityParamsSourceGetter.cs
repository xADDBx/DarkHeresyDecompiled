using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7cc04be6252f6c04aa31d33cfcd0a0da")]
public class CheckAbilityParamsSourceGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WarhammerAbilityParamsSource ParamsStouce;

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Ability?.Blueprint.AbilityParamsSource == ParamsStouce;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability params source is {ParamsStouce}";
	}
}
