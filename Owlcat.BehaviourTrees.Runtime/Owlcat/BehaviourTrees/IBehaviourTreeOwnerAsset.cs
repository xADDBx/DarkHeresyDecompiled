namespace Owlcat.BehaviourTrees;

public interface IBehaviourTreeOwnerAsset
{
	string AssetGuid { get; }

	string GetTitle();

	BehaviourTreeSerializableData GetData();

	void SetDirty();
}
