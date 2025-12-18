using System;
using Kingmaker.Framework.Pathfinding;
using Pathfinding;
using Unity.Collections;

namespace Kingmaker.Pathfinding;

public sealed class AdditionalGraphData : IDisposable
{
	public struct PerNodeData
	{
		public GridMeshNode MeshMode;

		public bool IsFloor;
	}

	public const int NoObstacleValue = int.MinValue;

	public readonly NativeArray<PerNodeData> NodeData;

	public readonly GridObstacleCache Obstacles;

	public readonly NavmeshCutsCache NavmeshCuts;

	public AdditionalGraphData(GridGraph graph)
	{
		NodeData = new NativeArray<PerNodeData>(graph.width * graph.depth, Allocator.Persistent);
		Obstacles = new GridObstacleCache(graph);
		NavmeshCuts = new NavmeshCutsCache(graph);
	}

	public void Dispose()
	{
		Obstacles.Dispose();
		NavmeshCuts.Dispose();
		NodeData.Dispose();
	}
}
