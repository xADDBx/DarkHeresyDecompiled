using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.Gameplay.Features.Scaling.Parts;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Gameplay.Features.Scaling.Utility;

public static class ScalingHelper
{
	[ThreadStatic]
	private static bool _inDescriptionFactLookup;

	public static ScalingInfo? GetScaling(this BlueprintAbilityWrapper ability, MechanicEntity? caster = null)
	{
		return caster?.GetOptional<PartOverrideScaling>()?.Get(ability) ?? GetScalingFromBlueprint(ability);
	}

	public static ScalingInfo? GetScaling(this AbilityData ability)
	{
		if (ability.ScalingOverride.HasValue)
		{
			return ability.ScalingOverride;
		}
		ScalingInfo? result = ability.Caster?.GetOptional<PartOverrideScaling>()?.Get(ability.Blueprint);
		if (result.HasValue)
		{
			return result;
		}
		foreach (BlueprintAbilityModifier item in ability.Modifiers.Reverse())
		{
			PropertyGetter[] array = item.ScalingCalculator?.Getters;
			if (array != null && array.Length > 0)
			{
				return new ScalingInfo(item.ScalingCalculator, item.ScalingDescription);
			}
		}
		return GetScalingFromBlueprint(ability.Blueprint);
	}

	private static ScalingInfo? GetScalingFromBlueprint(BlueprintAbilityWrapper ability)
	{
		PropertyScalingComponent propertyScalingComponent = ability.GetComponents<PropertyScalingComponent>().LastOrDefault();
		if (propertyScalingComponent != null)
		{
			return new ScalingInfo(propertyScalingComponent.Calculator, propertyScalingComponent.Description);
		}
		AbilityPropertyComponent component = ability.GetComponent<AbilityPropertyComponent>();
		if (component != null)
		{
			PropertyGetter[] array = component.ScalingCalculator?.Getters;
			if (array != null && array.Length > 0)
			{
				return new ScalingInfo(component.ScalingCalculator, component.ScalingDescription);
			}
		}
		return null;
	}

	public static ScalingInfo? GetScaling(this BlueprintMechanicEntityFact fact, MechanicEntity? caster = null)
	{
		ScalingInfo? result = caster?.GetOptional<PartOverrideScaling>()?.Get(fact);
		if (result.HasValue)
		{
			return result;
		}
		if (fact is BlueprintAbility && caster != null)
		{
			AbilityData abilityData = caster.Facts.Get<Ability>(fact)?.Data;
			if (abilityData != null)
			{
				return abilityData.GetScaling();
			}
		}
		if (fact is BlueprintAbilityModifier blueprintAbilityModifier)
		{
			PropertyGetter[] array = blueprintAbilityModifier.ScalingCalculator?.Getters;
			if (array != null && array.Length > 0)
			{
				return new ScalingInfo(blueprintAbilityModifier.ScalingCalculator, blueprintAbilityModifier.ScalingDescription);
			}
		}
		if (fact is BlueprintToggleAbility ability && caster != null)
		{
			IEnumerable<BlueprintAbilityModifier> enumerable = caster.GetOptional<PartAbilityModifiers>()?.GetBoundModifiers(ability);
			if (enumerable != null)
			{
				foreach (BlueprintAbilityModifier item in enumerable.Reverse())
				{
					PropertyGetter[] array = item.ScalingCalculator?.Getters;
					if (array != null && array.Length > 0)
					{
						return new ScalingInfo(item.ScalingCalculator, item.ScalingDescription);
					}
				}
			}
		}
		PropertyScalingComponent component = fact.GetComponent<PropertyScalingComponent>();
		if (component != null)
		{
			return new ScalingInfo(component.Calculator, component.Description);
		}
		AbilityPropertyComponent component2 = fact.GetComponent<AbilityPropertyComponent>();
		if (component2 != null)
		{
			PropertyGetter[] array = component2.ScalingCalculator?.Getters;
			if (array != null && array.Length > 0)
			{
				return new ScalingInfo(component2.ScalingCalculator, component2.ScalingDescription);
			}
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

	public static ScalingInfo? GetScalingFromDescriptionFact(BlueprintMechanicEntityFact fact, MechanicEntity? caster = null)
	{
		if (_inDescriptionFactLookup)
		{
			return null;
		}
		_inDescriptionFactLookup = true;
		try
		{
			BlueprintComponent[] componentsArray = fact.ComponentsArray;
			for (int i = 0; i < componentsArray.Length; i++)
			{
				if (!(componentsArray[i] is AbilityPropertyComponent { Entries: var entries }))
				{
					continue;
				}
				for (int j = 0; j < entries.Length; j++)
				{
					BlueprintMechanicEntityFact blueprintMechanicEntityFact = entries[j].UISettings?.DescriptionFact?.Get();
					if (blueprintMechanicEntityFact != null && blueprintMechanicEntityFact != fact)
					{
						ScalingInfo? scaling = blueprintMechanicEntityFact.GetScaling(caster);
						if (scaling.HasValue)
						{
							return scaling;
						}
					}
				}
			}
			return null;
		}
		finally
		{
			_inDescriptionFactLookup = false;
		}
	}
}
