using System;

namespace Kingmaker.EntitySystem.Stats.Base;

public readonly struct StatBaseValue : IEquatable<StatBaseValue>
{
	public readonly int Value;

	public readonly bool Enabled;

	public readonly bool Forced;

	public StatBaseValue(int value, bool enabled = true, bool forced = false)
	{
		Value = value;
		Enabled = enabled;
		Forced = forced;
	}

	public static implicit operator StatBaseValue(int value)
	{
		return new StatBaseValue(value);
	}

	public bool Equals(StatBaseValue other)
	{
		if (Value == other.Value && Enabled == other.Enabled)
		{
			return Forced == other.Forced;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is StatBaseValue other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Value, Enabled, Forced);
	}

	public override string ToString()
	{
		string arg = (Enabled ? "enabled" : "disabled");
		string arg2 = (Forced ? "forced" : "not forced");
		return $"{Value} ({arg}, {arg2})";
	}
}
