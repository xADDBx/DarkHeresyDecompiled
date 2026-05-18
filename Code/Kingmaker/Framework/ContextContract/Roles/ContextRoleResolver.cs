using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class ContextRoleResolver
{
	private sealed class ComponentRoleEntry
	{
		public IReadOnlyDictionary<ContextField, ContextRoleHint> ClassHints { get; }

		public IReadOnlyDictionary<string, Dictionary<ContextField, ContextRoleHint>> PerField { get; }

		public ComponentRoleEntry(IReadOnlyDictionary<ContextField, ContextRoleHint> classHints, IReadOnlyDictionary<string, Dictionary<ContextField, ContextRoleHint>> perField)
		{
			ClassHints = classHints;
			PerField = perField;
		}
	}

	private static readonly ConcurrentDictionary<Type, ComponentRoleEntry> _ComponentCache = new ConcurrentDictionary<Type, ComponentRoleEntry>();

	public static ContextRoleHint Resolve([CanBeNull] Type componentType, ContextEntryPointKind kind, [CanBeNull] string fieldName, ContextField field, [CanBeNull] Type ruleType = null)
	{
		ContextRoleHint contextRoleHint = ContextRoleHint.Empty;
		ContextRoleHint contextRoleHint2 = ContextRoleHint.Empty;
		if (componentType != null)
		{
			ComponentRoleEntry orBuild = GetOrBuild(componentType);
			if (fieldName != null && orBuild.PerField.TryGetValue(fieldName, out var value) && value.TryGetValue(field, out var value2))
			{
				contextRoleHint = value2;
			}
			if (orBuild.ClassHints.TryGetValue(field, out var value3))
			{
				contextRoleHint2 = value3;
			}
		}
		ContextRoleHint contextRoleHint3 = ContextEntryPointRoles.For(kind)[field];
		IReadOnlyList<ContextRoleFallback> source = ContextFieldFallbacks.Chain(field);
		string text = ((!string.IsNullOrEmpty(contextRoleHint.Primary)) ? contextRoleHint.Primary : ((!string.IsNullOrEmpty(contextRoleHint2.Primary)) ? contextRoleHint2.Primary : ((!string.IsNullOrEmpty(contextRoleHint3.Primary)) ? contextRoleHint3.Primary : null)));
		string text2 = ((!string.IsNullOrEmpty(contextRoleHint.Note)) ? contextRoleHint.Note : ((!string.IsNullOrEmpty(contextRoleHint2.Note)) ? contextRoleHint2.Note : ((!string.IsNullOrEmpty(contextRoleHint3.Note)) ? contextRoleHint3.Note : null)));
		List<ContextRoleFallback> chain = null;
		AppendDistinct(ref chain, contextRoleHint.Chain);
		AppendDistinct(ref chain, contextRoleHint2.Chain);
		AppendDistinct(ref chain, contextRoleHint3.Chain);
		AppendDistinct(ref chain, source);
		if (ruleType != null && (field == ContextField.RuleInitiator || field == ContextField.RuleTarget))
		{
			ContextRoleHint contextRoleHint4 = RuleProvenanceRegistry.Get(ruleType).For(field);
			if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(contextRoleHint4.Primary))
			{
				text = contextRoleHint4.Primary;
			}
			if (string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(contextRoleHint4.Note))
			{
				text2 = contextRoleHint4.Note;
			}
			AppendDistinct(ref chain, contextRoleHint4.Chain);
		}
		if (string.IsNullOrEmpty(text) && (chain == null || chain.Count == 0) && string.IsNullOrEmpty(text2))
		{
			return ContextRoleHint.Empty;
		}
		return new ContextRoleHint(text, chain, text2);
	}

	public static IReadOnlyList<ContextRoleFallback> ParseFallbacks([CanBeNull] string fallsBackTo)
	{
		if (string.IsNullOrEmpty(fallsBackTo))
		{
			return Array.Empty<ContextRoleFallback>();
		}
		string[] array = fallsBackTo.Split(',');
		List<ContextRoleFallback> list = new List<ContextRoleFallback>(array.Length);
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].Trim();
			if (text.Length != 0)
			{
				if (Enum.TryParse<ContextField>(text, ignoreCase: false, out var result))
				{
					list.Add(new ContextRoleFallback(result));
				}
				else
				{
					list.Add(new ContextRoleFallback(text));
				}
			}
		}
		return list;
	}

	public static void ClearCache()
	{
		_ComponentCache.Clear();
	}

	private static ComponentRoleEntry GetOrBuild(Type componentType)
	{
		return _ComponentCache.GetOrAdd(componentType, BuildEntry);
	}

	private static ComponentRoleEntry BuildEntry(Type componentType)
	{
		Dictionary<ContextField, ContextRoleHint> dictionary = new Dictionary<ContextField, ContextRoleHint>();
		foreach (ContextRoleAttribute customAttribute in componentType.GetCustomAttributes<ContextRoleAttribute>(inherit: true))
		{
			IReadOnlyList<ContextRoleFallback> chain = ParseFallbacks(customAttribute.FallsBackTo);
			dictionary[customAttribute.Field] = new ContextRoleHint(customAttribute.Primary, chain, customAttribute.Note);
		}
		Dictionary<string, Dictionary<ContextField, ContextRoleHint>> dictionary2 = new Dictionary<string, Dictionary<ContextField, ContextRoleHint>>();
		foreach (ContextRoleForFieldAttribute customAttribute2 in componentType.GetCustomAttributes<ContextRoleForFieldAttribute>(inherit: true))
		{
			if (!dictionary2.TryGetValue(customAttribute2.FieldName, out var value))
			{
				value = new Dictionary<ContextField, ContextRoleHint>();
				dictionary2[customAttribute2.FieldName] = value;
			}
			IReadOnlyList<ContextRoleFallback> chain2 = ParseFallbacks(customAttribute2.FallsBackTo);
			value[customAttribute2.Field] = new ContextRoleHint(customAttribute2.Primary, chain2, customAttribute2.Note);
		}
		return new ComponentRoleEntry(dictionary, dictionary2);
	}

	private static void AppendDistinct(ref List<ContextRoleFallback> chain, IReadOnlyList<ContextRoleFallback> source)
	{
		if (source == null || source.Count == 0)
		{
			return;
		}
		for (int i = 0; i < source.Count; i++)
		{
			ContextRoleFallback contextRoleFallback = source[i];
			if (!contextRoleFallback.HasContent)
			{
				continue;
			}
			if (chain == null)
			{
				chain = new List<ContextRoleFallback>();
			}
			bool flag = false;
			for (int j = 0; j < chain.Count; j++)
			{
				if (chain[j] == contextRoleFallback)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				chain.Add(contextRoleFallback);
			}
		}
	}
}
