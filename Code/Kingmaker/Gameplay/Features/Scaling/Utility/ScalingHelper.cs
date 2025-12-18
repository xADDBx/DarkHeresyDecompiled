using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Gameplay.Features.Scaling.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Gameplay.Features.Scaling.Utility;

public static class ScalingHelper
{
	public static ScalingInfo? GetScaling(this BlueprintAbilityWrapper ability, MechanicEntity? caster = null)
	{
		ScalingInfo? result = caster?.GetOptional<PartOverrideScaling>()?.Get(ability);
		if (result.HasValue)
		{
			return result;
		}
		PropertyScalingComponent propertyScalingComponent = ability.GetComponents<PropertyScalingComponent>().LastOrDefault();
		if (propertyScalingComponent != null)
		{
			return new ScalingInfo(propertyScalingComponent.Calculator, propertyScalingComponent.Description);
		}
		return null;
	}

	public static ScalingInfo? GetScaling(this AbilityData ability)
	{
		return ability.Blueprint.GetScaling(ability.Caster);
	}

	public static ScalingInfo? GetScaling(this BlueprintMechanicEntityFact fact, MechanicEntity? caster = null)
	{
		ScalingInfo? result = caster?.GetOptional<PartOverrideScaling>()?.Get(fact);
		if (result.HasValue)
		{
			return result;
		}
		PropertyScalingComponent component = fact.GetComponent<PropertyScalingComponent>();
		if (component != null)
		{
			return new ScalingInfo(component.Calculator, component.Description);
		}
		return null;
	}

	public static ScalingInfo? GetScaling(this MechanicEntityFact fact)
	{
		if (!(fact is Ability ability))
		{
			return fact.Blueprint.GetScaling(fact.Owner);
		}
		return ability.Data.GetScaling();
	}
}
