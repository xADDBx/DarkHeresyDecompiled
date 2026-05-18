using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Kingmaker.Localization.Shared;

[Serializable]
public sealed class UsageData
{
	[NotNull]
	[JsonProperty(PropertyName = "file_path")]
	public string FilePath = "";

	[CanBeNull]
	[JsonProperty(PropertyName = "scene_object_path", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string SceneObjectPath;

	[NotNull]
	[JsonProperty(PropertyName = "property_path")]
	public string PropertyPath = "";

	[NotNull]
	[JsonProperty(PropertyName = "global_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string GlobalObjectId = "";

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			if (obj is UsageData other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	private bool Equals(UsageData other)
	{
		if (FilePath == other.FilePath && SceneObjectPath == other.SceneObjectPath && PropertyPath == other.PropertyPath)
		{
			return GlobalObjectId == other.GlobalObjectId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(FilePath, SceneObjectPath, PropertyPath, GlobalObjectId);
	}
}
