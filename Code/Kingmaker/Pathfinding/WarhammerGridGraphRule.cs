using System;
using Owlcat.Runtime.Core.Collections;
using Pathfinding;
using Pathfinding.Graphs.Grid;
using Pathfinding.Graphs.Grid.Rules;
using Pathfinding.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerGridGraphRule : GridGraphRule
{
	[BurstCompile]
	private struct ApplyNavmeshMasksJob : IJob, GridIterationUtilities.INodeModifier
	{
		public IntBounds bounds;

		[ReadOnly]
		public NativeArray<float4> nodeNormals;

		public NavmeshMasksGeneration navmeshMasks;

		public NativeArray<bool> nodeWalkable;

		public NativeArray<AdditionalGraphData.PerNodeData> perNodeData;

		public void Execute()
		{
			GridIterationUtilities.ForEachNode(bounds.size, nodeNormals, ref this);
		}

		public void ModifyNode(int dataIndex, int dataX, int dataLayer, int dataZ)
		{
			int x = bounds.min.x + dataX;
			int z = bounds.min.z + dataZ;
			if (navmeshMasks.IsAdded(x, z))
			{
				nodeWalkable[dataIndex] = true;
			}
			else if (navmeshMasks.IsRemoved(x, z))
			{
				nodeWalkable[dataIndex] = false;
			}
			ref bool isFloor = ref UnsafeCollectionExtensions.ElementAsRef(in perNodeData, dataIndex).IsFloor;
			isFloor = navmeshMasks.IsFloor(x, z);
		}
	}

	[BurstCompile]
	private struct CutConnectionsJob : IJob, GridIterationUtilities.IConnectionFilter
	{
		public IntBounds bounds;

		[ReadOnly]
		public NativeArray<int> connectionCuts;

		public bool layeredDataLayout;

		public int width;

		public NativeArray<ulong> nodeConnections;

		public void Execute()
		{
			GridIterationUtilities.FilterNodeConnections(bounds, nodeConnections, layeredDataLayout, ref this);
		}

		public bool IsValidConnection(int dataIndex, int dataX, int dataLayer, int dataZ, int direction, int neighbourDataIndex)
		{
			int num = bounds.min.x + dataX;
			int num2 = bounds.min.z + dataZ;
			int index = num + num2 * width;
			int num3 = connectionCuts[index];
			int num4 = 1 << direction;
			return (num3 & num4) == 0;
		}
	}

	[BurstCompile]
	private struct NavmeshCutJob : IJob, GridIterationUtilities.INodeModifier
	{
		public IntBounds bounds;

		[ReadOnly]
		public NativeArray<float4> nodeNormals;

		[ReadOnly]
		public NativeArray<Rect> navmeshCuts;

		public NativeArray<bool> nodeWalkable;

		public void Execute()
		{
			GridIterationUtilities.ForEachNode(bounds.size, nodeNormals, ref this);
		}

		public void ModifyNode(int dataIndex, int dataX, int dataLayer, int dataZ)
		{
			int num = bounds.min.x + dataX;
			int num2 = bounds.min.z + dataZ;
			Rect other = new Rect(new Vector2(num, num2), Vector2.one);
			for (int i = 0; i < navmeshCuts.Length; i++)
			{
				if (navmeshCuts[i].Overlaps(other))
				{
					nodeWalkable[dataIndex] = false;
					break;
				}
			}
		}
	}

	public bool ShowGridCoordinates;

	private const float EdgeWalkableCheckOffset = 0.6f;

	private static readonly Vector3[] Offsets = new Vector3[4]
	{
		new Vector3(0.3f, 0f, 0.3f),
		new Vector3(0.3f, 0f, -0.3f),
		new Vector3(-0.3f, 0f, 0.3f),
		new Vector3(-0.3f, 0f, -0.3f)
	};

	public override void Register(GridGraphRules rules)
	{
		rules.AddMainThreadPass(Pass.BeforeCollision, OnBeforeCollision_MainThread);
		rules.AddJobSystemPass(Pass.BeforeConnections, OnBeforeConnections_Job);
		rules.AddJobSystemPass(Pass.AfterConnections, OnAfterConnections_Job);
	}

	private static void OnBeforeCollision_MainThread(GridGraphRules.Context context)
	{
		ModifyPositionAndWalkabilityUsingCornerRaycasts(context);
	}

	private static void OnBeforeConnections_Job(GridGraphRules.Context context)
	{
		AdditionalGraphData gridData = AdditionalGraphDataManager.Instance.GetGridData();
		NavmeshMasks component = AstarPath.active.gameObject.GetComponent<NavmeshMasks>();
		if (component != null)
		{
			NavmeshMasksGeneration navmeshMasksGeneration = component.ConvertData();
			if (navmeshMasksGeneration != default(NavmeshMasksGeneration))
			{
				ApplyNavmeshMasksJob data = default(ApplyNavmeshMasksJob);
				data.bounds = context.data.nodes.bounds;
				data.nodeWalkable = context.data.nodes.walkable;
				data.nodeNormals = context.data.nodes.normals;
				data.navmeshMasks = navmeshMasksGeneration;
				data.perNodeData = gridData.NodeData;
				JobHandle handle = data.Schedule(context.tracker);
				navmeshMasksGeneration.Dispose(handle);
			}
		}
		if (context.graph.Width * context.graph.Depth != context.data.nodes.walkable.Length)
		{
			NavmeshCutJob data2 = default(NavmeshCutJob);
			data2.bounds = context.data.nodes.bounds;
			data2.nodeWalkable = context.data.nodes.walkable;
			data2.nodeNormals = context.data.nodes.normals;
			data2.navmeshCuts = gridData.NavmeshCuts.Bounds;
			data2.Schedule(context.tracker);
		}
	}

	private static void OnAfterConnections_Job(GridGraphRules.Context context)
	{
		if (context.graph.Width * context.graph.Depth != context.data.nodes.walkable.Length)
		{
			AdditionalGraphData gridData = AdditionalGraphDataManager.Instance.GetGridData();
			CutConnectionsJob data = default(CutConnectionsJob);
			data.bounds = context.data.nodes.bounds;
			data.nodeConnections = context.data.nodes.connections;
			data.connectionCuts = gridData.Obstacles.ConnectionCuts;
			data.layeredDataLayout = context.data.nodes.layeredDataLayout;
			data.width = context.graph.width;
			data.Schedule(context.tracker);
		}
	}

	private static void ModifyPositionAndWalkabilityUsingCornerRaycasts(GridGraphRules.Context context)
	{
		GridGraph graph = context.graph;
		NativeArray<bool> walkable = context.data.nodes.walkable;
		NativeArray<Vector3> positions = context.data.nodes.positions;
		float num = Mathf.Sin(graph.maxSlope * (MathF.PI / 180f));
		for (int i = 0; i < positions.Length; i++)
		{
			if (walkable[i])
			{
				bool flag = true;
				Vector3 vector = positions[i];
				vector.y = 0f;
				float num2 = positions[i].y * 4f;
				for (int j = 0; j < Offsets.Length; j++)
				{
					Vector3 position = vector + Offsets[j] * graph.nodeSize;
					RaycastHit hit;
					bool walkable2;
					Vector3 vector2 = graph.collision.CheckHeight(position, out hit, out walkable2, 200f);
					num2 += vector2.y;
					flag = flag && walkable2;
					float f = Vector3.Dot((vector2 - positions[i]).normalized, Vector3.up);
					flag &= Mathf.Abs(f) < num;
				}
				num2 /= (float)(Offsets.Length + 4);
				vector.y = (flag ? num2 : positions[i].y);
				walkable[i] = flag;
				positions[i] = vector;
			}
		}
	}
}
