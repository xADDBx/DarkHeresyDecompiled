using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract.Roles;

public static class RuleProvenanceRegistry
{
	private const string UnknownRolePlaceholder = "(unknown)";

	private static readonly ConcurrentDictionary<Type, RuleProvenance> _Cache = new ConcurrentDictionary<Type, RuleProvenance>();

	public static RuleProvenance Get([NotNull] Type ruleType)
	{
		return _Cache.GetOrAdd(ruleType, BuildEntry);
	}

	public static void ClearCache()
	{
		_Cache.Clear();
	}

	private static RuleProvenance BuildEntry(Type ruleType)
	{
		RuleRolesAttribute customAttribute = ruleType.GetCustomAttribute<RuleRolesAttribute>(inherit: true);
		if (customAttribute == null)
		{
			return new RuleProvenance(ruleType, null, new ContextRoleHint("(unknown)"), new ContextRoleHint("(unknown)"));
		}
		IReadOnlyList<ContextRoleFallback> readOnlyList = ContextRoleResolver.ParseFallbacks(customAttribute.InitiatorFallsBackTo);
		IReadOnlyList<ContextRoleFallback> readOnlyList2 = ContextRoleResolver.ParseFallbacks(customAttribute.TargetFallsBackTo);
		ContextRoleHint initiatorHint = ((string.IsNullOrEmpty(customAttribute.Initiator) && readOnlyList.Count == 0) ? new ContextRoleHint("(unknown)") : new ContextRoleHint(customAttribute.Initiator, readOnlyList, customAttribute.Note));
		ContextRoleHint targetHint = ((string.IsNullOrEmpty(customAttribute.Target) && readOnlyList2.Count == 0) ? new ContextRoleHint("(unknown)") : new ContextRoleHint(customAttribute.Target, readOnlyList2, customAttribute.Note));
		return new RuleProvenance(ruleType, customAttribute.DisplayName, initiatorHint, targetHint, customAttribute.Note);
	}
}
