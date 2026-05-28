using System.Collections.Generic;
using System.Diagnostics;

namespace Owlcat.BehaviourTrees;

public class TrackingProfiler : BehaviourTreeProfiler
{
	private readonly List<BehaviourTreeNode> m_TempNodes = new List<BehaviourTreeNode>();

	private readonly Stopwatch m_TreesStopwatch = new Stopwatch();

	private readonly Dictionary<BehaviourTree, TreeProfilingData> m_TreesProfilingData = new Dictionary<BehaviourTree, TreeProfilingData>();

	private readonly Stopwatch m_NodesStopwatch = new Stopwatch();

	private readonly Dictionary<BehaviourTreeNode, NodeProfilingData> m_NodesProfilingData = new Dictionary<BehaviourTreeNode, NodeProfilingData>();

	private BehaviourTree m_CurrentTree;

	private BehaviourTreeNode m_CurrentNode;

	public override void BeginTick(BehaviourTree tree)
	{
		m_CurrentTree = tree;
		m_TreesStopwatch.Restart();
		if (!m_TreesProfilingData.ContainsKey(tree))
		{
			m_TreesProfilingData.Add(tree, new TreeProfilingData());
		}
	}

	public override void EndTick(BehaviourTree tree)
	{
		m_TreesStopwatch.Stop();
		double totalMilliseconds = m_TreesStopwatch.Elapsed.TotalMilliseconds;
		m_TreesProfilingData[tree].AddTickTime(totalMilliseconds);
		m_CurrentTree = null;
	}

	public override void AddRunningNodeTick(BehaviourTree tree)
	{
		m_TreesProfilingData[m_CurrentTree].AddRunningNodeTick();
	}

	public override TreeProfilingData GetProfilingData(BehaviourTree tree)
	{
		return m_TreesProfilingData.GetValueOrDefault(tree);
	}

	public override void BeginVisit(BehaviourTreeNode node)
	{
		m_CurrentNode = node;
		m_NodesStopwatch.Restart();
		if (!m_NodesProfilingData.ContainsKey(node))
		{
			m_NodesProfilingData.Add(node, new NodeProfilingData());
		}
	}

	public override void EndVisit(BehaviourTreeNode node)
	{
		m_NodesStopwatch.Stop();
		double totalMilliseconds = m_NodesStopwatch.Elapsed.TotalMilliseconds;
		m_NodesProfilingData[m_CurrentNode].AddTime(totalMilliseconds);
		m_CurrentNode = null;
		m_TreesProfilingData[m_CurrentTree].AddPassedNode();
	}

	public override NodeProfilingData GetProfilingData(BehaviourTreeNode node)
	{
		return m_NodesProfilingData.GetValueOrDefault(node);
	}

	public override void Reset(BehaviourTreeRuntimeToBlueprintBridge runtimeBridge)
	{
		runtimeBridge.FetchAllHierarchyNodes(m_TempNodes);
		foreach (BehaviourTreeNode tempNode in m_TempNodes)
		{
			m_NodesProfilingData.Remove(tempNode);
		}
		m_TempNodes.Clear();
		m_TreesProfilingData.Remove(runtimeBridge.BehaviourTree);
	}
}
