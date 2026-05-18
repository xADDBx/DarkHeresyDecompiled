using System;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7d93bf408e9add444bba5c95145a6942")]
public class CheckAbilityIsLinkedToModifierGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BpRef<BlueprintAbilityModifier> modifier;

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Ability?.Blueprint.AllModifiers.Contains(modifier) ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Used ability is linked to this modifier";
	}
}
