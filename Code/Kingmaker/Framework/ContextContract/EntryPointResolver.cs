using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Framework.ContextContract;

public sealed class EntryPointResolver
{
	private static readonly Dictionary<string, ContextEntryPointKind> _FallbackByTypeFullName = new Dictionary<string, ContextEntryPointKind>
	{
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityCasterRestriction",
			ContextEntryPointKind.AbilityCastRestriction
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityRestriction",
			ContextEntryPointKind.AbilityCastRestriction
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityOnCastLogic",
			ContextEntryPointKind.AbilityOnCast
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetRestriction",
			ContextEntryPointKind.AbilityTargetValidation
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.AbilityDeliverEffect",
			ContextEntryPointKind.AbilityDelivery
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySelectTarget",
			ContextEntryPointKind.AbilityPatternTargetSelection
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.AbilityApplyEffect",
			ContextEntryPointKind.AbilityApplyEffect
		},
		{
			"Kingmaker.Framework.Abilities.Components.AbilityAdditionalTargets",
			ContextEntryPointKind.AbilityAdditionalTargets
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.AbilityIgnoreLosForTarget",
			ContextEntryPointKind.AbilityLoSRangeException
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.Base.AbilityUnrestrictedRangeForTarget",
			ContextEntryPointKind.AbilityLoSRangeException
		},
		{
			"Kingmaker.UnitLogic.Abilities.Components.AbilityHaloEffect",
			ContextEntryPointKind.AbilityHaloOuter
		}
	};

	private readonly Dictionary<Type, ContextEntryPointKind[]> _cache = new Dictionary<Type, ContextEntryPointKind[]>();

	public ContextEntryPointKind[] Resolve(BlueprintComponent component)
	{
		if (component == null)
		{
			return Array.Empty<ContextEntryPointKind>();
		}
		Type type = component.GetType();
		if (_cache.TryGetValue(type, out var value))
		{
			return value;
		}
		ContextEntryPointKind[] array = ResolveUncached(type);
		_cache[type] = array;
		return array;
	}

	private static ContextEntryPointKind[] ResolveUncached(Type type)
	{
		HashSet<ContextEntryPointKind> hashSet = new HashSet<ContextEntryPointKind>();
		Type type2 = type;
		while (type2 != null)
		{
			EntryPointAttribute entryPointAttribute = (EntryPointAttribute)Attribute.GetCustomAttribute(type2, typeof(EntryPointAttribute), inherit: false);
			if (entryPointAttribute != null)
			{
				hashSet.Add(entryPointAttribute.Kind);
			}
			if (_FallbackByTypeFullName.TryGetValue(type2.FullName ?? string.Empty, out var value))
			{
				hashSet.Add(value);
			}
			type2 = type2.BaseType;
		}
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type3 in interfaces)
		{
			EntryPointAttribute entryPointAttribute2 = (EntryPointAttribute)Attribute.GetCustomAttribute(type3, typeof(EntryPointAttribute), inherit: false);
			if (entryPointAttribute2 != null)
			{
				hashSet.Add(entryPointAttribute2.Kind);
			}
			if (_FallbackByTypeFullName.TryGetValue(type3.FullName ?? string.Empty, out var value2))
			{
				hashSet.Add(value2);
			}
			if (type3.IsGenericType)
			{
				Type genericTypeDefinition = type3.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(IRulebookHandler<>) || genericTypeDefinition == typeof(IInitiatorRulebookHandler<>) || genericTypeDefinition == typeof(ITargetRulebookHandler<>) || genericTypeDefinition == typeof(IGlobalRulebookHandler<>))
				{
					hashSet.Add(ContextEntryPointKind.BuffComponentRulebookHandler);
				}
			}
		}
		if (hashSet.Count != 0)
		{
			return hashSet.ToArray();
		}
		return Array.Empty<ContextEntryPointKind>();
	}
}
