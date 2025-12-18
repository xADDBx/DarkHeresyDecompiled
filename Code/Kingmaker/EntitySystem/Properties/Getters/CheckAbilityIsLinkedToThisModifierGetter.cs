using System;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ff339fe80a6e5294b879f4af4d328a9f")]
public class CheckAbilityIsLinkedToThisModifierGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return this.GetAbility()?.Blueprint.AllModifiers.Contains(base.Owner as BlueprintAbilityModifier) ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Used ability is linked to this modifier";
	}
}
