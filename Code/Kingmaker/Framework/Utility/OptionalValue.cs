using System;

namespace Kingmaker.Framework.Utility;

[Serializable]
public abstract class OptionalValue
{
	public bool Enabled;
}
[Serializable]
public sealed class OptionalValue<T> : OptionalValue
{
	public T Value;

	public OptionalValue()
	{
	}

	public OptionalValue(T value, bool enabled)
	{
		Value = value;
		Enabled = enabled;
	}
}
