using System;
using Kingmaker.Blueprints.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintReferenceBase : IEquatable<BlueprintReferenceBase>, IReferenceBase
{
	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	[OwlPackInclude]
	protected string guid;

	private BlueprintScriptableObject Cached { get; set; }

	[JsonIgnore]
	public string Guid => guid;

	[JsonIgnore]
	public SimpleBlueprint Blueprint => GetBlueprint();

	protected BlueprintReferenceBase()
	{
	}

	public BlueprintScriptableObject GetBlueprint()
	{
		if (Cached == null)
		{
			Cached = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintScriptableObject;
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

	public bool IsEmpty()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return !GetBlueprint();
		}
		return true;
	}

	public static TRef CreateTyped<TRef>(BlueprintScriptableObject bp) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public bool Equals(BlueprintReferenceBase other)
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

	public override bool Equals(object obj)
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
			return Equals((BlueprintReferenceBase)obj);
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

	public static TRef CreateCopy<TRef>(TRef source) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = source.guid
		};
	}
}
