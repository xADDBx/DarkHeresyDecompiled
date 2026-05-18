using JetBrains.Annotations;

namespace Kingmaker.Framework.ContextContract.Roles;

public readonly struct ContextRoleFallback
{
	public ContextField? Source { get; }

	[CanBeNull]
	public string Description { get; }

	public bool HasContent
	{
		get
		{
			if (!Source.HasValue)
			{
				return !string.IsNullOrEmpty(Description);
			}
			return true;
		}
	}

	public ContextRoleFallback(ContextField source)
		: this(source, null)
	{
	}

	public ContextRoleFallback([NotNull] string description)
		: this(null, description)
	{
	}

	public ContextRoleFallback(ContextField? source, [CanBeNull] string description)
	{
		Source = source;
		Description = description;
	}

	public override string ToString()
	{
		if (Source.HasValue && Description == null)
		{
			return Source.Value.ToString();
		}
		if (Source.HasValue)
		{
			return $"{Source.Value} ({Description})";
		}
		return Description ?? string.Empty;
	}

	public bool Equals(ContextRoleFallback other)
	{
		if (Source == other.Source)
		{
			return Description == other.Description;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ContextRoleFallback other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)(Source.HasValue ? (Source.Value + 1) : ContextField.Caster) * 397) ^ (Description?.GetHashCode() ?? 0);
	}

	public static bool operator ==(ContextRoleFallback a, ContextRoleFallback b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(ContextRoleFallback a, ContextRoleFallback b)
	{
		return !a.Equals(b);
	}
}
