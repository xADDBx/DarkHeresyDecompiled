using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyCalculatorComponentHelper
{
	public static int CalculateValue(this BlueprintScriptableObject blueprint, ContextPropertyName propertyName, MechanicEntity currentEntity, IEvalContext context)
	{
		if (!blueprint.TryCalculateValue(propertyName, currentEntity, context, out var result))
		{
			PFLog.Default.ErrorWithReport($"Can't find local property '{propertyName}' in blueprint {blueprint.name}");
		}
		return result;
	}

	public static IEnumerable<IPropertyCalculatorComponent> GetAllCalculators(this IEvalContext context, BlueprintScriptableObject blueprint = null)
	{
		if (blueprint == null)
		{
			blueprint = context.Blueprint;
		}
		if (!(blueprint is BlueprintAbilityModifier))
		{
			ReadonlyList<BlueprintAbilityModifier>? readonlyList = context.Ability?.Blueprint.AllModifiers;
			if (readonlyList.HasValue)
			{
				ReadonlyList<BlueprintAbilityModifier> modifiers = readonlyList.GetValueOrDefault();
				for (int i = modifiers.Count - 1; i >= 0; i--)
				{
					foreach (IPropertyCalculatorComponent allCalculator in context.GetAllCalculators(modifiers[i]))
					{
						yield return allCalculator;
					}
				}
			}
		}
		BlueprintComponent[] componentsArray = blueprint.ComponentsArray;
		foreach (BlueprintComponent component in componentsArray)
		{
			if (component is AreaEffectClusterComponent areaEffectClusterComponent)
			{
				foreach (IPropertyCalculatorComponent allCalculator2 in context.GetAllCalculators(areaEffectClusterComponent.ClusterLogicBlueprint))
				{
					yield return allCalculator2;
				}
			}
			if (component is IPropertyCalculatorComponent propertyCalculatorComponent)
			{
				yield return propertyCalculatorComponent;
			}
			if (!(component is IPropertyCalculatorProvider propertyCalculatorProvider))
			{
				continue;
			}
			foreach (IPropertyCalculatorComponent propertyCalculator in propertyCalculatorProvider.GetPropertyCalculators())
			{
				yield return propertyCalculator;
			}
		}
	}

	public static bool TryCalculateValue(this BlueprintScriptableObject blueprint, ContextPropertyName propertyName, MechanicEntity currentEntity, IEvalContext context, out int result)
	{
		if (!(blueprint is BlueprintAbilityModifier))
		{
			ReadonlyList<BlueprintAbilityModifier>? readonlyList = context.Ability?.Blueprint.AllModifiers;
			if (readonlyList.HasValue)
			{
				ReadonlyList<BlueprintAbilityModifier> valueOrDefault = readonlyList.GetValueOrDefault();
				for (int num = valueOrDefault.Count - 1; num >= 0; num--)
				{
					if (valueOrDefault[num].TryCalculateValue(propertyName, currentEntity, context, out result))
					{
						return true;
					}
				}
			}
		}
		BlueprintComponent[] componentsArray = blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent is AreaEffectClusterComponent areaEffectClusterComponent && areaEffectClusterComponent.ClusterLogicBlueprint.TryCalculateValue(propertyName, currentEntity, context, out result))
			{
				return true;
			}
			if (blueprintComponent is IPropertyCalculatorComponent propertyCalculatorComponent && propertyCalculatorComponent.Name == propertyName)
			{
				result = CalculateValue(currentEntity, context, propertyCalculatorComponent);
				return true;
			}
			if (!(blueprintComponent is IPropertyCalculatorProvider propertyCalculatorProvider))
			{
				continue;
			}
			foreach (IPropertyCalculatorComponent propertyCalculator in propertyCalculatorProvider.GetPropertyCalculators())
			{
				if (propertyCalculator.Name == propertyName)
				{
					result = CalculateValue(currentEntity, context, propertyCalculator);
					return true;
				}
			}
		}
		result = 0;
		return false;
	}

	private static int CalculateValue(MechanicEntity currentEntity, IEvalContext context, IPropertyCalculatorComponent calculator)
	{
		return calculator.GetValue(context, currentEntity);
	}
}
