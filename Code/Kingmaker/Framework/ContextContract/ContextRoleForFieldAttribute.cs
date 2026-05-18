using System;
using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ContextRoleForFieldAttribute : Attribute
{
	[NotNull]
	public string FieldName { get; }

	public ContextField Field { get; }

	[NotNull]
	public string Primary { get; }

	[CanBeNull]
	public string Note { get; set; }

	[CanBeNull]
	public string FallsBackTo { get; set; }

	public ContextRoleForFieldAttribute([NotNull] string fieldName, ContextField field, [NotNull] string primary)
	{
		FieldName = fieldName;
		Field = field;
		Primary = primary;
	}
}
