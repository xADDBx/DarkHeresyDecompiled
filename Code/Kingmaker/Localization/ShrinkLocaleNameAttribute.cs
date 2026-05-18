using System;
using System.Linq;

namespace Kingmaker.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = true)]
public sealed class ShrinkLocaleNameAttribute : Attribute
{
	public readonly string[] FieldNameEntriesToShrink;

	public ShrinkLocaleNameAttribute()
	{
	}

	public ShrinkLocaleNameAttribute(params string[] fieldNameEntriesToShrink)
	{
		FieldNameEntriesToShrink = fieldNameEntriesToShrink.OrderByDescending((string s) => s.Length).ToArray();
	}
}
