using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Framework.ContextContract.Roles;

namespace Kingmaker.Framework.ContextContract;

public readonly struct RoleWalkRecord
{
	[NotNull]
	public BlueprintComponent Component { get; }

	public ContextEntryPointKind Kind { get; }

	public ContextRoleTable Roles { get; }

	[NotNull]
	public RuleProvenance[] RuleProvenances { get; }

	[NotNull]
	public ValidationIssue[] Issues { get; }

	[NotNull]
	public PropertyPickResolution[] PropertyPickResolutions { get; }

	public bool IsKindInferredFromBlueprintType { get; }

	public RoleWalkRecord([NotNull] BlueprintComponent component, ContextEntryPointKind kind, ContextRoleTable roles, [NotNull] RuleProvenance[] ruleProvenances, [NotNull] ValidationIssue[] issues, [NotNull] PropertyPickResolution[] propertyPickResolutions, bool isKindInferredFromBlueprintType = false)
	{
		Component = component ?? throw new ArgumentNullException("component");
		Kind = kind;
		Roles = roles;
		RuleProvenances = ruleProvenances ?? throw new ArgumentNullException("ruleProvenances");
		Issues = issues ?? throw new ArgumentNullException("issues");
		PropertyPickResolutions = propertyPickResolutions ?? throw new ArgumentNullException("propertyPickResolutions");
		IsKindInferredFromBlueprintType = isKindInferredFromBlueprintType;
	}
}
