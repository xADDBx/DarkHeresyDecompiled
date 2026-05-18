using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace Owlcat.Fmw.Blueprints;

[Serializable]
public sealed class BpRef<T> : BpRef where T : SimpleBlueprint
{
	public new T? MaybeBlueprint => base.MaybeBlueprint as T;

	public new T Blueprint => MaybeBlueprint ?? throw new NullReferenceException();

	public BpRef(string id)
		: base(id)
	{
	}

	public BpRef(T blueprint)
		: base(blueprint)
	{
	}

	public BpRef()
	{
	}

	public bool Is(SimpleBlueprint? bp)
	{
		if (bp == null)
		{
			return IsNull();
		}
		return guid == bp.AssetGuid;
	}

	public static implicit operator T?(BpRef<T>? reference)
	{
		if ((object)reference == null)
		{
			return null;
		}
		return reference.MaybeBlueprint;
	}

	public override string ToString()
	{
		return guid;
	}

	private bool Equals(BpRef<T> other)
	{
		return Equals((BpRef)other);
	}

	public override bool Equals(object? obj)
	{
		if (this != obj)
		{
			if (obj is BpRef<T> other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(BpRef<T>? @ref, T? blueprint)
	{
		return @ref?.Is(blueprint) ?? (blueprint == null);
	}

	public static bool operator !=(BpRef<T>? @ref, T? blueprint)
	{
		return !(@ref == blueprint);
	}
}
[Serializable]
public class BpRef : IEquatable<BpRef>, IReferenceBase
{
	[JsonProperty]
	[SerializeField]
	[HideInInspector]
	protected string? guid;

	private SimpleBlueprint? Cached { get; set; }

	[JsonIgnore]
	public string Guid => guid ?? string.Empty;

	public SimpleBlueprint? MaybeBlueprint => GetBlueprint();

	public SimpleBlueprint Blueprint => GetBlueprint() ?? throw new NullReferenceException();

	public BpRef(string id)
	{
		guid = id;
	}

	public BpRef(SimpleBlueprint blueprint)
	{
		guid = blueprint.AssetGuid;
	}

	public BpRef()
	{
	}

	private SimpleBlueprint? GetBlueprint()
	{
		if (string.IsNullOrEmpty(guid))
		{
			return null;
		}
		if (Cached == null)
		{
			Cached = ResourcesLibrary.TryGetBlueprint(guid);
		}
		return Cached;
	}

	public bool IsNull()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return GetBlueprint() == null;
		}
		return true;
	}

	public bool Equals(BpRef other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return guid == other.guid;
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((BpRef)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (guid == null)
		{
			return 0;
		}
		return guid.GetHashCode();
	}

	public void ReadGuidFromJson(string value)
	{
		guid = value;
	}

	public static implicit operator SimpleBlueprint?(BpRef @ref)
	{
		return @ref.GetBlueprint();
	}
}
