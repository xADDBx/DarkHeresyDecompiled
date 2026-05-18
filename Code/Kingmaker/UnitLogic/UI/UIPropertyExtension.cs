using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.UnitLogic.UI;

public static class UIPropertyExtension
{
	private static IEnumerable<UIProperty> GetUIProperties(this BlueprintMechanicEntityFact fact, [CanBeNull] IEnumerable<BlueprintMechanicEntityFact> additionalFacts, [CanBeNull] MechanicEntity owner, [CanBeNull] IEvalContext context, [CanBeNull] ItemEntity item)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionFactBlueprint = fact;
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)owner;
			IEnumerable<UIPropertySettings> enumerable = fact.GetComponents<UIPropertiesComponent>()?.SelectMany((UIPropertiesComponent i) => i.Properties).Concat(additionalFacts?.SelectMany((BlueprintMechanicEntityFact i) => i.GetComponents<UIPropertiesComponent>()?.SelectMany((UIPropertiesComponent j) => j.Properties)) ?? Enumerable.Empty<UIPropertySettings>());
			if (enumerable == null)
			{
				yield break;
			}
			if (context == null)
			{
				context = EvalContext.Current;
			}
			foreach (UIPropertySettings item2 in enumerable)
			{
				UIPropertyName nameType = item2.NameType;
				string name = item2.Name;
				string description = item2.Description;
				BlueprintMechanicEntityFact descriptionFact = item2.DescriptionFact;
				int? propertyValue = null;
				if (item2.PropertyName.HasValue && owner != null && (item2.PropertySource ?? fact).TryCalculateValue(item2.PropertyName.Value, owner, context, out var result))
				{
					propertyValue = result;
				}
				yield return new UIProperty(nameType, name, description, descriptionFact, propertyValue);
			}
			foreach (AbilityPropertyEntry item3 in EnumerateAbilityPropertyEntries(fact, additionalFacts))
			{
				AbilityPropertyUISettings uISettings = item3.UISettings;
				if (uISettings == null || !uISettings.Enabled)
				{
					continue;
				}
				int? propertyValue2 = null;
				if (owner != null)
				{
					PropertyGetter[] getters = item3.Value.Getters;
					if (getters != null && getters.Length > 0 && fact.TryCalculateValue(item3.Name, owner, context, out var result2))
					{
						propertyValue2 = result2;
					}
				}
				yield return new UIProperty(item3.UISettings.NameType, item3.UISettings.Name, item3.UISettings.Description, item3.UISettings.DescriptionFact, propertyValue2);
			}
		}
	}

	public static IEnumerable<UIProperty> GetUIProperties(this MechanicEntityFact fact, ItemEntity item)
	{
		return fact.Blueprint.GetUIProperties(null, fact.Owner, EvalContext.Current, item);
	}

	public static IEnumerable<UIProperty> GetUIProperties(this Ability ability, ItemEntity item = null)
	{
		using (ability.Data.ClaimExecutionContext(ability.Data.Caster))
		{
			foreach (UIProperty uIProperty in ability.Data.GetUIProperties(item))
			{
				yield return uIProperty;
			}
		}
	}

	public static IEnumerable<UIProperty> GetUIProperties(this AbilityData ability, ItemEntity item = null)
	{
		using (ability.ClaimExecutionContext(ability.Caster))
		{
			using (GameLogContext.Scope)
			{
				using (EvalContext.Build().Ability(ability).Push())
				{
					GameLogContext.DescriptionAbility = ability;
					foreach (UIProperty uIProperty in ability.Blueprint.OriginalBlueprint.GetUIProperties(ability.Blueprint.AllModifiers, ability.Caster, EvalContext.Current, item ?? ability.Weapon))
					{
						yield return uIProperty;
					}
				}
			}
		}
	}

	private static IEnumerable<AbilityPropertyEntry> EnumerateAbilityPropertyEntries(BlueprintMechanicEntityFact fact, [CanBeNull] IEnumerable<BlueprintMechanicEntityFact> additionalFacts)
	{
		AbilityPropertyComponent component = fact.GetComponent<AbilityPropertyComponent>();
		if (component != null)
		{
			AbilityPropertyEntry[] entries = component.Entries;
			for (int i = 0; i < entries.Length; i++)
			{
				yield return entries[i];
			}
		}
		if (additionalFacts == null)
		{
			yield break;
		}
		foreach (BlueprintMechanicEntityFact additionalFact in additionalFacts)
		{
			component = additionalFact.GetComponent<AbilityPropertyComponent>();
			if (component != null)
			{
				AbilityPropertyEntry[] entries = component.Entries;
				for (int i = 0; i < entries.Length; i++)
				{
					yield return entries[i];
				}
			}
		}
	}
}
