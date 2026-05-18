using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Scaling.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAreaEffect))]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Scaling/AbilityPropertyComponent")]
[TypeId("31ba486e0baad0a4895500c30f1e56ed")]
public class AbilityPropertyComponent : BlueprintComponent, IPropertyCalculatorProvider
{
	private readonly struct EntryProxy : IPropertyCalculatorComponent
	{
		private readonly AbilityPropertyEntry _entry;

		public ContextPropertyName Name => _entry.Name;

		public SaveToContextType SaveToContext => _entry.SaveToContext;

		public EntryProxy(AbilityPropertyEntry entry)
		{
			_entry = entry;
		}

		public int GetValue(IEvalContext context, MechanicEntity currentEntity)
		{
			return _entry.Value.GetValue(currentEntity, context);
		}
	}

	[Header("Scaling")]
	public PropertyCalculator ScalingCalculator;

	public LocalizedString ScalingDescription;

	[Header("Properties")]
	public AbilityPropertyEntry[] Entries = Array.Empty<AbilityPropertyEntry>();

	[NonSerialized]
	private AbilityPropertyEntry[] _cachedEntries;

	[NonSerialized]
	private IPropertyCalculatorComponent[] _cachedProxies;

	public static AbilityPropertyEntry FindEntryByLinkKey(BlueprintMechanicEntityFact blueprintFact, string linkKey)
	{
		if (blueprintFact == null)
		{
			return null;
		}
		BlueprintComponent[] componentsArray = blueprintFact.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (!(componentsArray[i] is AbilityPropertyComponent { Entries: var entries }))
			{
				continue;
			}
			for (int j = 0; j < entries.Length; j++)
			{
				if (!string.IsNullOrEmpty(entries[j].UISettings?.LinkKey) && entries[j].UISettings.LinkKey == linkKey)
				{
					return entries[j];
				}
			}
		}
		return null;
	}

	public IReadOnlyList<IPropertyCalculatorComponent> GetPropertyCalculators()
	{
		if (_cachedProxies == null || _cachedEntries != Entries)
		{
			_cachedEntries = Entries;
			int num = 0;
			for (int i = 0; i < Entries.Length; i++)
			{
				PropertyGetter[] array = Entries[i].Value?.Getters;
				if (array != null && array.Length > 0)
				{
					num++;
				}
			}
			_cachedProxies = new IPropertyCalculatorComponent[num];
			int num2 = 0;
			for (int j = 0; j < Entries.Length; j++)
			{
				PropertyGetter[] array = Entries[j].Value?.Getters;
				if (array != null && array.Length > 0)
				{
					_cachedProxies[num2++] = new EntryProxy(Entries[j]);
				}
			}
		}
		return _cachedProxies;
	}
}
