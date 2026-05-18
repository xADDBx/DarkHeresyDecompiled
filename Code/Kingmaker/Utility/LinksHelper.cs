using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Tutorial.Solvers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.UI;
using UnityEngine;

namespace Kingmaker.Utility;

public static class LinksHelper
{
	[CanBeNull]
	public static BaseUnitEntity GetUnit(string id)
	{
		return EntityService.Instance.GetEntity<BaseUnitEntity>(id);
	}

	[CanBeNull]
	public static ItemEntity GetItem(string id)
	{
		return EntityService.Instance.GetEntity<ItemEntity>(id);
	}

	[CanBeNull]
	public static BlueprintItem GetBlueprintItem(string id)
	{
		return Utilities.GetBlueprint<BlueprintItem>(id);
	}

	[CanBeNull]
	public static BlueprintMechanicEntityFact GetMechanicFact(string id)
	{
		return ResourcesLibrary.TryGetBlueprint(id) as BlueprintMechanicEntityFact;
	}

	[CanBeNull]
	public static BlueprintAnswer GetAnswer(string id)
	{
		return ResourcesLibrary.TryGetBlueprint(id) as BlueprintAnswer;
	}

	public static BlueprintAbilityTag GetAbilityTag(string id)
	{
		return ResourcesLibrary.TryGetBlueprint(id) as BlueprintAbilityTag;
	}

	[CanBeNull]
	public static AbilityData GetPartyAbility(string id)
	{
		IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
		while (enumerator.MoveNext())
		{
			if (enumerator.Current?.UniqueId == id)
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	[CanBeNull]
	public static UIPropertySettings GetUISettings(string[] keys)
	{
		return GetMechanicFact(keys[1])?.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings settings) => settings.LinkKey == keys[2]);
	}

	public static StatTooltipData GetStatData(string stat, string unitId)
	{
		if (string.IsNullOrEmpty(stat) || string.IsNullOrEmpty(unitId))
		{
			return null;
		}
		BaseUnitEntity unit = GetUnit(unitId);
		if (unit == null)
		{
			return null;
		}
		StatType? statType = UtilityUnit.TryGetStatType(stat);
		if (!statType.HasValue)
		{
			return null;
		}
		return StatTooltipData.FromActor(unit, statType.Value);
	}

	public static string GetUIPropertySettingsReferenceLink(string linkKey, BlueprintMechanicEntityFact blueprint)
	{
		if (linkKey.IsNullOrEmpty())
		{
			return string.Empty;
		}
		return "{uip|" + linkKey + "|" + blueprint.AssetGuid + "}";
	}

	public static string UpdateLink(string link, BlueprintMechanicEntityFact blueprintFact, MechanicEntity calculationSource, out LocalizedString usedString)
	{
		if (calculationSource == null)
		{
			return UpdateLink(link, blueprintFact, out usedString);
		}
		return UpdateLinkWithSource(link, blueprintFact, calculationSource, out usedString);
	}

	private static string UpdateLinkWithSource(string link, BlueprintMechanicEntityFact blueprintFact, MechanicEntity calculationSource, out LocalizedString usedString)
	{
		UIPropertySettings property = (blueprintFact?.GetComponent<UIPropertiesComponent>())?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
		if (property != null)
		{
			BlueprintMechanicEntityFact blueprintMechanicEntityFact = property.PropertySource ?? blueprintFact;
			MechanicEntityFact mechanicEntityFact = GetMechanicEntityFact(calculationSource, blueprintMechanicEntityFact) ?? GetMechanicEntityFact(calculationSource, blueprintFact);
			IPropertyCalculatorComponent propertyCalculatorComponent = blueprintMechanicEntityFact.GetComponents<IPropertyCalculatorComponent>().FirstOrDefault((IPropertyCalculatorComponent c) => c.Name == property.PropertyName);
			using MechanicsContext mechanicsContext = ((mechanicEntityFact == null) ? MechanicsContext.Claim(blueprintMechanicEntityFact, calculationSource) : null);
			int? num = propertyCalculatorComponent?.GetValue(mechanicEntityFact?.MaybeContext ?? mechanicsContext, calculationSource);
			string glossaryMechanicsHTML = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
			if (num.HasValue)
			{
				link = $"<b><color={glossaryMechanicsHTML}><link=\"uip:{blueprintFact.AssetGuid}:{link}:{calculationSource.UniqueId}\">{Mathf.Abs(num.Value)}</link></color></b>";
			}
		}
		else
		{
			if (TryResolveAbilityPropertyLinkForEditor(blueprintFact, link, calculationSource, out var resolvedLink, out usedString))
			{
				return resolvedLink;
			}
			property = blueprintFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == link);
			if (property != null)
			{
				usedString = property.Description;
				return usedString;
			}
		}
		usedString = null;
		return link;
	}

	private static string UpdateLink(string link, BlueprintMechanicEntityFact blueprintFact, out LocalizedString usedString)
	{
		using (GameLogContext.Scope)
		{
			UIPropertySettings uIPropertySettings = blueprintFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
			if (uIPropertySettings != null)
			{
				GameLogContext.DescriptionFactBlueprint = uIPropertySettings.PropertySource ?? blueprintFact;
				usedString = uIPropertySettings.Description;
				return ((string)usedString).TrimEnd('%');
			}
			AbilityPropertyEntry abilityPropertyEntry = AbilityPropertyComponent.FindEntryByLinkKey(blueprintFact, link);
			if (abilityPropertyEntry?.UISettings != null)
			{
				GameLogContext.DescriptionFactBlueprint = blueprintFact;
				usedString = abilityPropertyEntry.UISettings.Description;
				return ((string)usedString).TrimEnd('%');
			}
			usedString = null;
			return link;
		}
	}

	internal static bool TryResolveAbilityPropertyLink(BlueprintMechanicEntityFact blueprintFact, string linkKey, MechanicEntity calculationSource, out string resolvedLink)
	{
		LocalizedString usedString;
		return TryResolveAbilityPropertyLinkCore(blueprintFact, linkKey, calculationSource, appendFormulaOnZero: true, out resolvedLink, out usedString);
	}

	internal static bool TryResolveAbilityPropertyLinkForEditor(BlueprintMechanicEntityFact blueprintFact, string linkKey, MechanicEntity calculationSource, out string resolvedLink, out LocalizedString usedString)
	{
		return TryResolveAbilityPropertyLinkCore(blueprintFact, linkKey, calculationSource, appendFormulaOnZero: false, out resolvedLink, out usedString);
	}

	private static bool TryResolveAbilityPropertyLinkCore(BlueprintMechanicEntityFact blueprintFact, string linkKey, MechanicEntity calculationSource, bool appendFormulaOnZero, out string resolvedLink, out LocalizedString usedString)
	{
		resolvedLink = null;
		usedString = null;
		AbilityPropertyEntry abilityPropertyEntry = AbilityPropertyComponent.FindEntryByLinkKey(blueprintFact, linkKey);
		if (abilityPropertyEntry?.UISettings == null)
		{
			return false;
		}
		PropertyGetter[] array = abilityPropertyEntry.Value?.Getters;
		if (array == null || array.Length <= 0)
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionFactBlueprint = blueprintFact;
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)calculationSource;
				usedString = abilityPropertyEntry.UISettings.Description;
				resolvedLink = ((string)usedString).TrimEnd('%');
				return true;
			}
		}
		MechanicEntityFact mechanicEntityFact = GetMechanicEntityFact(calculationSource, blueprintFact);
		using MechanicsContext mechanicsContext = ((mechanicEntityFact == null) ? MechanicsContext.Claim(blueprintFact, calculationSource) : null);
		if (blueprintFact.TryCalculateValue(abilityPropertyEntry.Name, calculationSource, mechanicEntityFact?.MaybeContext ?? mechanicsContext, out var result))
		{
			string glossaryMechanicsHTML = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
			string text = ((appendFormulaOnZero && result == 0) ? (" " + ReadEntryDescriptionInScope(blueprintFact, abilityPropertyEntry, calculationSource)) : string.Empty);
			resolvedLink = $"<b><color={glossaryMechanicsHTML}><link=\"uip:{blueprintFact.AssetGuid}:{linkKey}:{calculationSource.UniqueId}\">{Mathf.Abs(result)}</link></color></b>{text}";
			return true;
		}
		return false;
	}

	private static string ReadEntryDescriptionInScope(BlueprintMechanicEntityFact blueprintFact, AbilityPropertyEntry entry, MechanicEntity calculationSource)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionFactBlueprint = blueprintFact;
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)calculationSource;
			return entry.UISettings.Description.Text;
		}
	}

	internal static MechanicEntityFact GetMechanicEntityFact(MechanicEntity mechanicEntity, BlueprintMechanicEntityFact blueprintFact)
	{
		return mechanicEntity.Facts.Get<MechanicEntityFact>(blueprintFact);
	}
}
