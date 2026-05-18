using System;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RuleRolesAttribute : Attribute
{
	[CanBeNull]
	public string Initiator { get; set; }

	[CanBeNull]
	public string Target { get; set; }

	[CanBeNull]
	public string InitiatorFallsBackTo { get; set; }

	[CanBeNull]
	public string TargetFallsBackTo { get; set; }

	[CanBeNull]
	public string Note { get; set; }

	[CanBeNull]
	public string DisplayName { get; set; }
}
