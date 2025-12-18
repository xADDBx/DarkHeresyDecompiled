using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Scaling.Getters;

[Serializable]
[TypeId("6181abb5a1af4f679dde4022ccf1407a")]
public sealed class AbilityScalingGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Scaling";
	}

	protected override int GetBaseValue()
	{
		SimpleBlueprint owner = base.Owner;
		AbilityData abilityData = ((owner is BlueprintAbility || owner is BlueprintAbilityModifier) ? this.GetAbility() : this.GetMechanicContext()?.SourceAbility);
		if (abilityData == null)
		{
			return 0;
		}
		ScalingInfo? scaling = abilityData.GetScaling();
		if (!scaling.HasValue)
		{
			return 0;
		}
		PropertyContext context = new PropertyContext(abilityData.Caster, base.PropertyContext.MechanicContext, null, null, abilityData);
		return scaling.Value.Calculator.GetValue(context);
	}
}
