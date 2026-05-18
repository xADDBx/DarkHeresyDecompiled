using System;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SetsContextScopeAttribute : Attribute
{
	public ContextEntryPointKind Kind { get; }

	public SetsContextScopeAttribute(ContextEntryPointKind kind)
	{
		Kind = kind;
	}
}
