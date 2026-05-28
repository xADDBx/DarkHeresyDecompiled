using System;

namespace Owlcat.BehaviourTrees;

public class EmptyProfiler : BehaviourTreeProfiler
{
	public override void BeginTick(BehaviourTree tree)
	{
	}

	public override void EndTick(BehaviourTree tree)
	{
	}

	public override void AddRunningNodeTick(BehaviourTree tree)
	{
	}

	public override TreeProfilingData GetProfilingData(BehaviourTree tree)
	{
		throw new NotImplementedException();
	}

	public override void BeginVisit(BehaviourTreeNode node)
	{
	}

	public override void EndVisit(BehaviourTreeNode node)
	{
	}

	public override NodeProfilingData GetProfilingData(BehaviourTreeNode node)
	{
		throw new NotImplementedException();
	}

	public override void Reset(BehaviourTreeRuntimeToBlueprintBridge runtimeBridge)
	{
	}
}
