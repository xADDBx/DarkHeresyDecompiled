using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.UnitLogic.UI;

public static class UIPropertyExtension
{
	private static IEnumerable<UIProperty> GetUIProperties(this BlueprintMechanicEntityFact fact, [CanBeNull] IEnumerable<BlueprintMechanicEntityFact> additionalFacts, [CanBeNull] MechanicEntity owner, [CanBeNull] MechanicsContext context, [CanBeNull] ItemEntity item)
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
				context = owner?.MainFact.MaybeContext;
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
		}
	}

	public static IEnumerable<UIProperty> GetUIProperties(this MechanicEntityFact fact, ItemEntity item)
	{
		return fact.Blueprint.GetUIProperties(null, fact.Owner, fact.MaybeContext, item);
	}

	public static IEnumerable<UIProperty> GetUIProperties(this Ability ability, ItemEntity item = null)
	{
		using (ability.Data.ClaimExecutionContext(ability.Data.Caster))
		{
			return ability.Data.GetUIProperties(item);
		}
	}

	public static IEnumerable<UIProperty> GetUIProperties(this AbilityData ability, ItemEntity item = null)
	{
		using AbilityExecutionContext context = ability.ClaimExecutionContext(ability.Caster);
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionAbility = ability;
			return ability.Blueprint.OriginalBlueprint.GetUIProperties(ability.Blueprint.AllModifiers, ability.Caster, context, item ?? ability.Weapon);
		}
	}
}
