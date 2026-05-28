namespace Owlcat.BehaviourTrees;

public interface IBehaviourTreeRuntimeDatabase
{
	BehaviourTreeSerializableData LoadById(string assetGuid);
}
