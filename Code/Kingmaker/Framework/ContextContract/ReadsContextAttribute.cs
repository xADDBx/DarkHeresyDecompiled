using System;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ReadsContextAttribute : Attribute
{
	public ContextField[] Fields { get; }

	public ReadsContextAttribute(params ContextField[] fields)
	{
		Fields = fields;
	}
}
