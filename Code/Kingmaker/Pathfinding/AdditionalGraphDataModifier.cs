using System.Collections;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class AdditionalGraphDataModifier : GraphModifier
{
	private const string MissingGridGraphErrorMessage = "AdditionalGraphDataModifier could not find GridGraph. Modifier should be on the same GameObject as graph component.";

	private BitArray m_UnwalkableNodesFromAreaMask;

	private static GridGraph GetGridGraph()
	{
		return AstarPath.active?.graphs.FirstItem() as GridGraph;
	}

	public override void OnPreScan()
	{
		GridGraph gridGraph = GetGridGraph();
		if (gridGraph == null)
		{
			PFLog.Pathfinding.Error("AdditionalGraphDataModifier could not find GridGraph. Modifier should be on the same GameObject as graph component.");
			return;
		}
		m_UnwalkableNodesFromAreaMask = new BitArray(gridGraph.Width * gridGraph.Depth);
		AdditionalGraphDataManager.Instance.SetGraphData(gridGraph.graphIndex, new AdditionalGraphData(gridGraph));
	}

	public override void OnLatePostScan()
	{
		GridGraph gridGraph = GetGridGraph();
		if (gridGraph == null)
		{
			PFLog.Pathfinding.Error("AdditionalGraphDataModifier could not find GridGraph. Modifier should be on the same GameObject as graph component.");
			return;
		}
		AdditionalGraphData graphData = AdditionalGraphDataManager.Instance.GetGraphData(gridGraph.graphIndex);
		new AdditionalGraphDataGenerator(gridGraph, graphData).Generate();
		RemoveUnreachableNodes(gridGraph);
		GraphUpdateRouter.OnLatePostScan();
	}

	public override void OnGraphsPostUpdate()
	{
		if (m_UnwalkableNodesFromAreaMask == null)
		{
			return;
		}
		for (int i = 0; i < m_UnwalkableNodesFromAreaMask.Count; i++)
		{
			if (m_UnwalkableNodesFromAreaMask[i])
			{
				AstarPath.active.data.gridGraph.nodes[i].Walkable = false;
			}
		}
	}

	private void RemoveUnreachableNodes(GridGraph g)
	{
		ValidNavmeshArea[] array = Object.FindObjectsByType<ValidNavmeshArea>(FindObjectsSortMode.None);
		HashSet<uint> hashSet = new HashSet<uint>();
		ValidNavmeshArea[] array2 = array;
		foreach (ValidNavmeshArea validNavmeshArea in array2)
		{
			NNInfo nearest = g.GetNearest(validNavmeshArea.transform.position, new NNConstraint
			{
				graphMask = 1 << (int)g.graphIndex
			});
			if (nearest.node != null)
			{
				hashSet.Add(nearest.node.Area);
			}
		}
		ApplyValidAreas(g, hashSet);
	}

	public void ApplyValidAreas(GridGraph g, HashSet<uint> validAreas)
	{
		GridNodeBase[] nodes = g.nodes;
		foreach (GridNodeBase gridNodeBase in nodes)
		{
			if (!validAreas.Contains(gridNodeBase.Area))
			{
				gridNodeBase.Walkable = false;
				((GridNode)gridNodeBase).SetAllConnectionInternal(0);
				m_UnwalkableNodesFromAreaMask[gridNodeBase.NodeInGridIndex] = true;
			}
		}
	}
}
