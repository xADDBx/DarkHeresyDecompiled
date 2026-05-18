using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d92b5c4b3f6b49ef9e138e8b4ae2b7aa")]
public class CheckAbilityBaseActionPointCostGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public int Cost;

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Ability?.Blueprint.ActionPointCost == Cost;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability costs {Cost} action points";
	}
}
