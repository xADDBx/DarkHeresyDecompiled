namespace Owlcat.BehaviourTrees;

public abstract class BehaviourTreeProfiler
{
	public abstract void BeginTick(BehaviourTree tree);

	public abstract void EndTick(BehaviourTree tree);

	public abstract void AddRunningNodeTick(BehaviourTree tree);

	public abstract TreeProfilingData GetProfilingData(BehaviourTree tree);

	public abstract void BeginVisit(BehaviourTreeNode node);

	public abstract void EndVisit(BehaviourTreeNode node);

	public abstract NodeProfilingData GetProfilingData(BehaviourTreeNode node);

	public abstract void Reset(BehaviourTreeRuntimeToBlueprintBridge runtimeBridge);
}
