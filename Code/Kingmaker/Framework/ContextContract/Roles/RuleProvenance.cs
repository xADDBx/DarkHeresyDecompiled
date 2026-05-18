using System;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract.Roles;

public readonly struct RuleProvenance
{
	[NotNull]
	public Type RuleType { get; }

	[NotNull]
	public string DisplayName { get; }

	public ContextRoleHint InitiatorHint { get; }

	public ContextRoleHint TargetHint { get; }

	[CanBeNull]
	public string Note { get; }

	public RuleProvenance([NotNull] Type ruleType, [CanBeNull] string displayName, ContextRoleHint initiatorHint, ContextRoleHint targetHint, [CanBeNull] string note = null)
	{
		RuleType = ruleType;
		DisplayName = (string.IsNullOrEmpty(displayName) ? ruleType.Name : displayName);
		InitiatorHint = initiatorHint;
		TargetHint = targetHint;
		Note = note;
	}

	public ContextRoleHint For(ContextField field)
	{
		return field switch
		{
			ContextField.RuleInitiator => InitiatorHint, 
			ContextField.RuleTarget => TargetHint, 
			_ => ContextRoleHint.Empty, 
		};
	}
}
