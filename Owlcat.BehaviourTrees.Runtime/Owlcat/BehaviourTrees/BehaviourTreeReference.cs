using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class BehaviourTreeReference
{
	public string AssetGuid;

	private BehaviourTreeSerializableData Cached;

	public BehaviourTreeSerializableData Get()
	{
		if (Cached == null || Cached.AssetGuid != AssetGuid)
		{
			Cached = BehaviourTreeRuntimeDatabaseWrapper.Instance.LoadById(AssetGuid);
		}
		return Cached;
	}

	public bool IsEmpty()
	{
		if (!string.IsNullOrEmpty(AssetGuid))
		{
			return !Get();
		}
		return true;
	}
}
