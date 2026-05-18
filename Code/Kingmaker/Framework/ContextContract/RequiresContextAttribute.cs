using System;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequiresContextAttribute : Attribute
{
	public ContextField[] Fields { get; }

	public RequiresContextAttribute(params ContextField[] fields)
	{
		Fields = fields;
	}
}
