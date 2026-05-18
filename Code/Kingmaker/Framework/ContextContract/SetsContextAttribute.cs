using System;

namespace Kingmaker.Framework.ContextContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class SetsContextAttribute : Attribute
{
	public ContextField Field { get; }

	public Availability Availability { get; }

	public SetsContextAttribute(ContextField field, Availability availability = Availability.Definitely)
	{
		Field = field;
		Availability = availability;
	}
}
