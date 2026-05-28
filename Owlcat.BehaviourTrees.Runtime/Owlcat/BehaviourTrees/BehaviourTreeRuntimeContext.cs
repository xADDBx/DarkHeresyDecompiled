using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public class BehaviourTreeRuntimeContext
{
	private readonly List<BehaviourTreeRuntimeToBlueprintBridge> m_RuntimeBridges = new List<BehaviourTreeRuntimeToBlueprintBridge>();

	public IReadOnlyList<BehaviourTreeRuntimeToBlueprintBridge> RuntimeBridges => m_RuntimeBridges;

	public LogicalBranchesRuntimeController LogicalBranchesRuntimeController { get; }

	public NodeErrorsRuntimeController NodeErrorsRuntimeController { get; }

	public BehaviourTreeProfiler Profiler { get; }

	public BehaviourTreeRuntimeContext()
	{
		LogicalBranchesRuntimeController = new LogicalBranchesRuntimeController();
		NodeErrorsRuntimeController = new NodeErrorsRuntimeController(this);
		Profiler = (BehaviourTreeConfig.Features.ProfilerEnabled ? ((BehaviourTreeProfiler)new TrackingProfiler()) : ((BehaviourTreeProfiler)new EmptyProfiler()));
	}

	public BehaviourTreeRuntimeToBlueprintBridge CreateBehaviourTree(BehaviourTreeSerializableData data)
	{
		BehaviourTreeRuntimeToBlueprintBridge behaviourTreeRuntimeToBlueprintBridge = new BehaviourTreeRuntimeToBlueprintBridge(this, data);
		m_RuntimeBridges.Add(behaviourTreeRuntimeToBlueprintBridge);
		return behaviourTreeRuntimeToBlueprintBridge;
	}

	public BehaviourTreeRuntimeToBlueprintBridge CreateBehaviourTree(ParameterizedBehaviourTree parameterizedBehaviourTree)
	{
		BehaviourTreeRuntimeToBlueprintBridge behaviourTreeRuntimeToBlueprintBridge = new BehaviourTreeRuntimeToBlueprintBridge(this, parameterizedBehaviourTree);
		m_RuntimeBridges.Add(behaviourTreeRuntimeToBlueprintBridge);
		return behaviourTreeRuntimeToBlueprintBridge;
	}

	public void Delete(BehaviourTreeRuntimeToBlueprintBridge runtimeBridge)
	{
		m_RuntimeBridges.Remove(runtimeBridge);
		Profiler.Reset(runtimeBridge);
	}
}
