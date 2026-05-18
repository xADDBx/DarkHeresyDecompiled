using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.Framework.TextTools;

public sealed class UIPropertyTemplate : TextTemplate
{
	public override int MinParameters => 2;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		string text = parameters[0];
		string assetId = parameters[1];
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			try
			{
				BlueprintMechanicEntityFact blueprintMechanicEntityFact = ResourcesLibrary.TryGetBlueprint<BlueprintMechanicEntityFact>(assetId);
				if (blueprintMechanicEntityFact == null)
				{
					return text;
				}
				MechanicEntity mechanicEntity = (MechanicEntity)GameLogContext.DescriptionOwner.Value;
				if (mechanicEntity != null)
				{
					return ResolveWithSource(blueprintMechanicEntityFact, text, mechanicEntity);
				}
				return ResolveWithoutSource(blueprintMechanicEntityFact, text);
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"{arg}");
				return text;
			}
		}
	}

	private static string ResolveWithSource(BlueprintMechanicEntityFact blueprintFact, string linkKey, MechanicEntity calculationSource)
	{
		if (LinksHelper.TryResolveAbilityPropertyLink(blueprintFact, linkKey, calculationSource, out var resolvedLink))
		{
			return resolvedLink;
		}
		UIPropertySettings property = blueprintFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == linkKey);
		if (property != null)
		{
			BlueprintMechanicEntityFact blueprintMechanicEntityFact = property.PropertySource ?? blueprintFact;
			MechanicEntityFact mechanicEntityFact = LinksHelper.GetMechanicEntityFact(calculationSource, blueprintMechanicEntityFact) ?? LinksHelper.GetMechanicEntityFact(calculationSource, blueprintFact);
			IPropertyCalculatorComponent propertyCalculatorComponent = blueprintMechanicEntityFact.GetComponents<IPropertyCalculatorComponent>().FirstOrDefault((IPropertyCalculatorComponent c) => c.Name == property.PropertyName);
			using MechanicsContext mechanicsContext = ((mechanicEntityFact == null) ? MechanicsContext.Claim(blueprintMechanicEntityFact, calculationSource) : null);
			int? num = propertyCalculatorComponent?.GetValue(mechanicEntityFact?.MaybeContext ?? mechanicsContext, calculationSource);
			string glossaryMechanicsHTML = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
			if (num.HasValue)
			{
				return $"<b><color={glossaryMechanicsHTML}><link=\"uip:{blueprintFact.AssetGuid}:{linkKey}:{calculationSource.UniqueId}\">{Mathf.Abs(num.Value)}</link></color></b>";
			}
		}
		property = blueprintFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == linkKey);
		if (property != null)
		{
			return property.Description;
		}
		return linkKey;
	}

	private static string ResolveWithoutSource(BlueprintMechanicEntityFact blueprintFact, string linkKey)
	{
		using (GameLogContext.Scope)
		{
			AbilityPropertyEntry abilityPropertyEntry = AbilityPropertyComponent.FindEntryByLinkKey(blueprintFact, linkKey);
			if (abilityPropertyEntry?.UISettings != null)
			{
				GameLogContext.DescriptionFactBlueprint = blueprintFact;
				return ((string)abilityPropertyEntry.UISettings.Description).TrimEnd('%');
			}
			UIPropertySettings uIPropertySettings = blueprintFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == linkKey);
			if (uIPropertySettings != null)
			{
				GameLogContext.DescriptionFactBlueprint = uIPropertySettings.PropertySource ?? blueprintFact;
				return ((string)uIPropertySettings.Description).TrimEnd('%');
			}
			return linkKey;
		}
	}
}
