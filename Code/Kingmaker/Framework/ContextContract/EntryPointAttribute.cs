using System;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class EntryPointAttribute : Attribute
{
	public ContextEntryPointKind Kind { get; }

	public EntryPointAttribute(ContextEntryPointKind kind)
	{
		Kind = kind;
	}
}
