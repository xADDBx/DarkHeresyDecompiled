using System;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ContextRoleAttribute : Attribute
{
	public ContextField Field { get; }

	[NotNull]
	public string Primary { get; }

	[CanBeNull]
	public string Note { get; set; }

	[CanBeNull]
	public string FallsBackTo { get; set; }

	public ContextRoleAttribute(ContextField field, [NotNull] string primary)
	{
		Field = field;
		Primary = primary;
	}
}
