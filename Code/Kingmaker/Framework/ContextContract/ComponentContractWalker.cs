using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Code.GameCore.ElementsSystem;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework.ContextContract.Roles;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Framework.ContextContract;

public sealed class ComponentContractWalker
{
	private readonly EntryPointResolver _resolver;

	private readonly Dictionary<Type, (ContextField[] Reads, ContextField[] Requires)> _readsCache = new Dictionary<Type, (ContextField[], ContextField[])>();

	private readonly Dictionary<Type, (ContextField Field, Availability Avail)[]> _providesCache = new Dictionary<Type, (ContextField, Availability)[]>();

	private readonly Dictionary<Type, FieldInfo[]> _childFieldsCache = new Dictionary<Type, FieldInfo[]>();

	private readonly Dictionary<Type, FieldInfo[]> _targetTypeFieldsCache = new Dictionary<Type, FieldInfo[]>();

	private readonly Dictionary<BlueprintComponent, List<PropertyPickResolution>> _resolutionsByComponent = new Dictionary<BlueprintComponent, List<PropertyPickResolution>>();

	private static readonly HashSet<Type> _CalculatorWrapperTypes = new HashSet<Type>
	{
		typeof(RestrictionCalculator),
		typeof(ContextValue),
		typeof(ContextDiceValue),
		typeof(ContextDurationValue)
	};

	public ComponentContractWalker(EntryPointResolver resolver = null)
	{
		_resolver = resolver ?? new EntryPointResolver();
	}

	public IEnumerable<ValidationIssue> Walk(BlueprintScriptableObject blueprint)
	{
		_resolutionsByComponent.Clear();
		if (blueprint == null)
		{
			yield break;
		}
		if (!(blueprint is BlueprintAbility blueprintAbility))
		{
			if (!(blueprint is BlueprintBuff blueprintBuff))
			{
				if (!(blueprint is BlueprintFeature blueprintFeature))
				{
					yield break;
				}
				foreach (ValidationIssue item in WalkComponentArray(blueprintFeature, blueprintFeature.ComponentsArray, useLayer7aFallback: true))
				{
					yield return item;
				}
				yield break;
			}
			foreach (ValidationIssue item2 in WalkComponentArray(blueprintBuff, blueprintBuff.ComponentsArray, useLayer7aFallback: true))
			{
				yield return item2;
			}
			yield break;
		}
		foreach (ValidationIssue item3 in WalkComponentArray(blueprintAbility, blueprintAbility.ComponentsArray, useLayer7aFallback: false))
		{
			yield return item3;
		}
	}

	public IEnumerable<RoleWalkRecord> WalkWithRoles(BlueprintScriptableObject blueprint)
	{
		return WalkWithRoles(blueprint, null);
	}

	public IEnumerable<RoleWalkRecord> WalkWithRoles(BlueprintScriptableObject blueprint, ContextEntryPointKind? overrideKindForInferredSites)
	{
		return WalkWithRoles(blueprint, overrideKindForInferredSites, null);
	}

	public IEnumerable<RoleWalkRecord> WalkWithRoles(BlueprintScriptableObject blueprint, ContextEntryPointKind? overrideKindForInferredSites, IReadOnlyList<RuleProvenance> transitiveRuleProvenances)
	{
		if (blueprint == null)
		{
			yield break;
		}
		ValidationIssue[] array = Walk(blueprint).ToArray();
		Dictionary<BlueprintComponent, List<ValidationIssue>> issuesByComponent = new Dictionary<BlueprintComponent, List<ValidationIssue>>();
		ValidationIssue[] array2 = array;
		foreach (ValidationIssue validationIssue in array2)
		{
			if (validationIssue?.Path != null && validationIssue.Path.Count >= 2 && validationIssue.Path[1].Element is BlueprintComponent key)
			{
				if (!issuesByComponent.TryGetValue(key, out var value))
				{
					value = (issuesByComponent[key] = new List<ValidationIssue>());
				}
				value.Add(validationIssue);
			}
		}
		BlueprintComponent[] componentsArray;
		if (!(blueprint is BlueprintAbility blueprintAbility))
		{
			if (!(blueprint is BlueprintBuff blueprintBuff))
			{
				if (!(blueprint is BlueprintFeature blueprintFeature))
				{
					yield break;
				}
				componentsArray = blueprintFeature.ComponentsArray;
			}
			else
			{
				componentsArray = blueprintBuff.ComponentsArray;
			}
		}
		else
		{
			componentsArray = blueprintAbility.ComponentsArray;
		}
		if (componentsArray == null)
		{
			yield break;
		}
		BlueprintComponent[] array3 = componentsArray;
		foreach (BlueprintComponent component in array3)
		{
			if (component == null)
			{
				continue;
			}
			ContextEntryPointKind[] array4 = TranslateResolverKindsForCarrier(_resolver.Resolve(component), blueprint);
			bool isInferred = false;
			if (array4.Length == 0)
			{
				ContextEntryPointKind? contextEntryPointKind = overrideKindForInferredSites ?? BlueprintTypeDefaultKinds.For(blueprint.GetType());
				if (!contextEntryPointKind.HasValue)
				{
					continue;
				}
				array4 = new ContextEntryPointKind[1] { contextEntryPointKind.Value };
				isInferred = true;
			}
			Type componentType = component.GetType();
			RuleProvenance[] direct = ResolveRuleProvenances(componentType);
			RuleProvenance[] ruleProvenances = MergeTransitiveProvenances(direct, transitiveRuleProvenances);
			issuesByComponent.TryGetValue(component, out var value2);
			ValidationIssue[] issuesArray = ((value2 == null) ? Array.Empty<ValidationIssue>() : value2.ToArray());
			_resolutionsByComponent.TryGetValue(component, out var value3);
			PropertyPickResolution[] resolutionsArray = ((value3 == null) ? Array.Empty<PropertyPickResolution>() : value3.ToArray());
			ContextEntryPointKind[] array5 = array4;
			foreach (ContextEntryPointKind kind in array5)
			{
				ContextRoleTable roles = BuildRoleTable(componentType, kind);
				yield return new RoleWalkRecord(component, kind, roles, ruleProvenances, issuesArray, resolutionsArray, isInferred);
			}
		}
	}

	private static ContextRoleTable BuildRoleTable(Type componentType, ContextEntryPointKind kind)
	{
		ContextRoleTable contextRoleTable = ContextEntryPointRoles.For(kind);
		ContextRoleTable result = ContextRoleTable.Empty;
		HashSet<ContextField> hashSet = new HashSet<ContextField>();
		foreach (ContextField nonEmptyField in contextRoleTable.NonEmptyFields)
		{
			hashSet.Add(nonEmptyField);
		}
		object[] customAttributes = componentType.GetCustomAttributes(typeof(ContextRoleAttribute), inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is ContextRoleAttribute contextRoleAttribute)
			{
				hashSet.Add(contextRoleAttribute.Field);
			}
		}
		foreach (ContextField item in hashSet)
		{
			ContextRoleHint hint = ContextRoleResolver.Resolve(componentType, kind, null, item);
			if (hint.HasContent)
			{
				result = result.With(item, hint);
			}
		}
		customAttributes = componentType.GetCustomAttributes(typeof(ContextRoleForFieldAttribute), inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is ContextRoleForFieldAttribute contextRoleForFieldAttribute)
			{
				ContextRoleHint hint2 = ContextRoleResolver.Resolve(componentType, kind, contextRoleForFieldAttribute.FieldName, contextRoleForFieldAttribute.Field);
				if (hint2.HasContent)
				{
					result = result.WithForField(contextRoleForFieldAttribute.FieldName, contextRoleForFieldAttribute.Field, hint2);
				}
			}
		}
		return result;
	}

	public static RuleProvenance[] ResolveRuleProvenances(Type componentType)
	{
		HashSet<Type> hashSet = null;
		List<RuleProvenance> list = null;
		Type[] interfaces = componentType.GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (!type.IsGenericType)
			{
				continue;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition != typeof(IRulebookHandler<>) && genericTypeDefinition != typeof(IInitiatorRulebookHandler<>) && genericTypeDefinition != typeof(ITargetRulebookHandler<>) && genericTypeDefinition != typeof(IGlobalRulebookHandler<>))
			{
				continue;
			}
			Type type2 = type.GetGenericArguments()[0];
			if (hashSet == null)
			{
				hashSet = new HashSet<Type>();
			}
			if (hashSet.Add(type2))
			{
				if (list == null)
				{
					list = new List<RuleProvenance>();
				}
				list.Add(RuleProvenanceRegistry.Get(type2));
			}
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return Array.Empty<RuleProvenance>();
	}

	private static ContextEntryPointKind[] TranslateResolverKindsForCarrier(ContextEntryPointKind[] resolverKinds, BlueprintScriptableObject carrier)
	{
		if (resolverKinds == null || resolverKinds.Length == 0)
		{
			return resolverKinds;
		}
		if (!(carrier is BlueprintFeature))
		{
			return resolverKinds;
		}
		ContextEntryPointKind[] array = null;
		for (int i = 0; i < resolverKinds.Length; i++)
		{
			if (resolverKinds[i] == ContextEntryPointKind.BuffComponentRulebookHandler)
			{
				if (array == null)
				{
					array = new ContextEntryPointKind[resolverKinds.Length];
					for (int j = 0; j < i; j++)
					{
						array[j] = resolverKinds[j];
					}
				}
				array[i] = ContextEntryPointKind.FeatureComponentRulebookHandler;
			}
			else if (array != null)
			{
				array[i] = resolverKinds[i];
			}
		}
		return array ?? resolverKinds;
	}

	private static RuleProvenance[] MergeTransitiveProvenances(RuleProvenance[] direct, IReadOnlyList<RuleProvenance> transitive)
	{
		if (transitive == null || transitive.Count == 0)
		{
			return direct;
		}
		HashSet<Type> hashSet = null;
		List<RuleProvenance> list = null;
		foreach (RuleProvenance item in transitive)
		{
			bool flag = false;
			foreach (RuleProvenance ruleProvenance in direct)
			{
				if (ruleProvenance.RuleType == item.RuleType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<Type>();
					list = new List<RuleProvenance>(direct.Length + transitive.Count);
					list.AddRange(direct);
				}
				if (hashSet.Add(item.RuleType))
				{
					list.Add(item);
				}
			}
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return direct;
	}

	private IEnumerable<ValidationIssue> WalkComponentArray(BlueprintScriptableObject root, BlueprintComponent[] components, bool useLayer7aFallback)
	{
		if (components == null)
		{
			yield break;
		}
		ContextEntryPointKind? fallbackKind = (useLayer7aFallback ? BlueprintTypeDefaultKinds.For(root?.GetType()) : null);
		foreach (BlueprintComponent component in components)
		{
			if (component == null)
			{
				continue;
			}
			ContextEntryPointKind[] array = TranslateResolverKindsForCarrier(_resolver.Resolve(component), root);
			if (array.Length == 0)
			{
				if (!fallbackKind.HasValue)
				{
					continue;
				}
				array = new ContextEntryPointKind[1] { fallbackKind.Value };
			}
			bool hasWarm = false;
			ContextEntryPointKind[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				if (!ContextEntryPointContracts.For(array2[j]).IsColdContext)
				{
					hasWarm = true;
					break;
				}
			}
			ContextEntryPointKind[] array3 = array;
			for (int k = 0; k < array3.Length; k++)
			{
				ContextContract contract = ContextEntryPointContracts.For(array3[k]);
				List<PathStep> path = new List<PathStep>
				{
					new PathStep(root),
					new PathStep(component)
				};
				bool skipDescent = hasWarm && contract.IsColdContext;
				foreach (ValidationIssue item in WalkElement(component, contract, path, skipDescent))
				{
					yield return item;
				}
			}
		}
	}

	private IEnumerable<ValidationIssue> WalkElement(object element, ContextContract contract, List<PathStep> path, bool skipDescent = false)
	{
		if (element == null)
		{
			yield break;
		}
		Type type = element.GetType();
		(ContextField[], ContextField[]) readsRequires = GetReadsRequires(type);
		ContextField[] reads = readsRequires.Item1;
		ContextField[] item = readsRequires.Item2;
		ContextField[] array = item;
		foreach (ContextField field in array)
		{
			foreach (ContextField item4 in ContextContract.ExpandDerived(field))
			{
				Availability availability = contract[item4];
				if (contract.IsColdContext)
				{
					yield return MakeIssue(ValidationIssue.SeverityLevel.Warning, element, path, item4, Availability.Definitely, availability, $"{type.Name} requires {item4} (from derived {field}) but the entry point has no ambient EvalContext (cold scenario).");
				}
				else if (availability != Availability.Definitely)
				{
					yield return MakeIssue(ValidationIssue.SeverityLevel.Error, element, path, item4, Availability.Definitely, availability, $"{type.Name} REQUIRES {item4} (from derived {field}) but the active contract provides only {availability}.");
				}
			}
		}
		array = reads;
		foreach (ContextField field in array)
		{
			foreach (ContextField item5 in ContextContract.ExpandDerived(field))
			{
				Availability availability2 = contract[item5];
				if (contract.IsColdContext)
				{
					yield return MakeIssue(ValidationIssue.SeverityLevel.Warning, element, path, item5, Availability.Maybe, availability2, $"{type.Name} reads {item5} (from derived {field}) in a cold-context scenario (no ambient EvalContext).");
				}
				else if (availability2 == Availability.Never)
				{
					yield return MakeIssue(ValidationIssue.SeverityLevel.Warning, element, path, item5, Availability.Maybe, availability2, $"{type.Name} reads {item5} (from derived {field}) but the active contract marks it Never.");
				}
			}
		}
		FieldInfo[] targetTypeFields = GetTargetTypeFields(type);
		foreach (FieldInfo fieldInfo in targetTypeFields)
		{
			if (!(fieldInfo.GetValue(element) is PropertyTargetType propertyTargetType))
			{
				continue;
			}
			ContextField contextField = PropertyTargetTypeMapping.ToContextField(propertyTargetType);
			ContextField[] array2 = PropertyTargetTypeMapping.EffectiveFallbackChain(propertyTargetType);
			ContextField? resolvedTo = null;
			ContextField[] array3 = array2;
			foreach (ContextField contextField2 in array3)
			{
				foreach (ContextField item6 in ContextContract.ExpandDerived(contextField2))
				{
					if (contract[item6] != 0)
					{
						resolvedTo = contextField2;
						break;
					}
				}
				if (resolvedTo.HasValue)
				{
					break;
				}
			}
			if (path.Count >= 2 && path[1].Element is BlueprintComponent key)
			{
				if (!_resolutionsByComponent.TryGetValue(key, out var value))
				{
					value = new List<PropertyPickResolution>();
					_resolutionsByComponent[key] = value;
				}
				value.Add(new PropertyPickResolution(type.Name, fieldInfo.Name, propertyTargetType, contextField, array2, resolvedTo));
			}
			if (!resolvedTo.HasValue)
			{
				yield return MakeIssue(ValidationIssue.SeverityLevel.Error, element, path, contextField, Availability.Definitely, Availability.Never, $"{type.Name}.{fieldInfo.Name} targets {propertyTargetType} ({contextField}) which is Never set in the active contract " + "and no caller-side fallback (" + string.Join(", ", array2) + ") is available either.");
			}
		}
		if (skipDescent)
		{
			yield break;
		}
		ContextContract childContract = contract;
		SetsContextScopeAttribute setsContextScopeAttribute = (SetsContextScopeAttribute)Attribute.GetCustomAttribute(type, typeof(SetsContextScopeAttribute), inherit: true);
		if (setsContextScopeAttribute != null)
		{
			childContract = ContextEntryPointContracts.For(setsContextScopeAttribute.Kind);
		}
		(ContextField, Availability)[] provides = GetProvides(type);
		for (int j = 0; j < provides.Length; j++)
		{
			(ContextField, Availability) tuple = provides[j];
			ContextField item2 = tuple.Item1;
			Availability item3 = tuple.Item2;
			childContract = childContract.With(item2, item3);
		}
		foreach (PathStep item7 in EnumerateChildren(element))
		{
			path.Add(item7);
			foreach (ValidationIssue item8 in WalkElement(item7.Element, childContract, path))
			{
				yield return item8;
			}
			path.RemoveAt(path.Count - 1);
		}
	}

	private (ContextField[] Reads, ContextField[] Requires) GetReadsRequires(Type type)
	{
		if (_readsCache.TryGetValue(type, out (ContextField[], ContextField[]) value))
		{
			return value;
		}
		ReadsContextAttribute[] source = (ReadsContextAttribute[])Attribute.GetCustomAttributes(type, typeof(ReadsContextAttribute), inherit: true);
		RequiresContextAttribute[] source2 = (RequiresContextAttribute[])Attribute.GetCustomAttributes(type, typeof(RequiresContextAttribute), inherit: true);
		ContextField[] item = source.SelectMany((ReadsContextAttribute a) => a.Fields).Distinct().ToArray();
		ContextField[] item2 = source2.SelectMany((RequiresContextAttribute a) => a.Fields).Distinct().ToArray();
		value = (item, item2);
		_readsCache[type] = value;
		return value;
	}

	private (ContextField Field, Availability Avail)[] GetProvides(Type type)
	{
		if (_providesCache.TryGetValue(type, out (ContextField, Availability)[] value))
		{
			return value;
		}
		value = ((SetsContextAttribute[])Attribute.GetCustomAttributes(type, typeof(SetsContextAttribute), inherit: true)).Select((SetsContextAttribute a) => (Field: a.Field, Availability: a.Availability)).ToArray();
		_providesCache[type] = value;
		return value;
	}

	private FieldInfo[] GetTargetTypeFields(Type type)
	{
		if (_targetTypeFieldsCache.TryGetValue(type, out var value))
		{
			return value;
		}
		value = (from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where f.FieldType == typeof(PropertyTargetType)
			select f).ToArray();
		_targetTypeFieldsCache[type] = value;
		return value;
	}

	private IEnumerable<PathStep> EnumerateChildren(object parent)
	{
		Type type = parent.GetType();
		if (!_childFieldsCache.TryGetValue(type, out var value))
		{
			value = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsChildCarrying).ToArray();
			_childFieldsCache[type] = value;
		}
		FieldInfo[] array = value;
		foreach (FieldInfo f in array)
		{
			object value2 = f.GetValue(parent);
			if (value2 == null)
			{
				continue;
			}
			if (value2 is ElementsList element)
			{
				yield return new PathStep(element, f.Name);
			}
			else if (value2 is IEnumerable enumerable && !(value2 is string))
			{
				int i = 0;
				foreach (object item in enumerable)
				{
					if (item != null && IsTrackedChild(item))
					{
						yield return new PathStep(item, $"{f.Name}[{i}]");
					}
					i++;
				}
			}
			else if (IsTrackedChild(value2))
			{
				yield return new PathStep(value2, f.Name);
			}
		}
	}

	private static bool IsChildCarrying(FieldInfo f)
	{
		Type type = f.FieldType;
		if (type.IsArray)
		{
			type = type.GetElementType();
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			type = type.GetGenericArguments()[0];
		}
		if (type == null)
		{
			return false;
		}
		if (!typeof(ElementsList).IsAssignableFrom(type) && !typeof(Element).IsAssignableFrom(type) && !typeof(BlueprintComponent).IsAssignableFrom(type) && !typeof(PropertyGetter).IsAssignableFrom(type))
		{
			return IsCalculatorWrapperType(type);
		}
		return true;
	}

	private static bool IsTrackedChild(object o)
	{
		if (!(o is ElementsList) && !(o is Element) && !(o is BlueprintComponent) && !(o is PropertyGetter))
		{
			if (o != null)
			{
				return IsCalculatorWrapperType(o.GetType());
			}
			return false;
		}
		return true;
	}

	private static bool IsCalculatorWrapperType(Type t)
	{
		return _CalculatorWrapperTypes.Contains(t);
	}

	private static ValidationIssue MakeIssue(ValidationIssue.SeverityLevel severity, object owner, List<PathStep> path, ContextField field, Availability required, Availability actual, string message)
	{
		return new ValidationIssue(severity, message, owner, path.ToArray(), field, required, actual);
	}
}
