using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.UI.AR;

internal static class CombatHudGraphDataSource
{
	public static GridGraph FindGraph()
	{
		AstarPath active = AstarPath.active;
		if (active == null)
		{
			return null;
		}
		if (active.graphs == null)
		{
			return null;
		}
		NavGraph[] graphs = active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is GridGraph gridGraph && IsGraphValid(gridGraph))
			{
				return gridGraph;
			}
		}
		return null;
	}

	public static bool IsGraphValid(GridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (graph.width <= 0)
		{
			return false;
		}
		if (graph.depth <= 0)
		{
			return false;
		}
		if (graph.nodes == null)
		{
			return false;
		}
		AdditionalGraphData graphData = AdditionalGraphDataManager.Instance.GetGraphData(graph.graphIndex);
		if (graphData == null)
		{
			return false;
		}
		int num = graph.width * graph.depth;
		if (num != graph.nodes.Length)
		{
			return false;
		}
		if (num != graphData.NodeData.Length)
		{
			return false;
		}
		return true;
	}
}
