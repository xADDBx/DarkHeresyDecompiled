using System;
using Pathfinding;
using Pathfinding.Graphs.Grid;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[JsonOptIn]
[Preserve]
public class CustomGridGraph : NavGraph
{
	public static int kDataFormatSignature = -12345;

	public static uint kDataFormatVersionLegacy = 0u;

	public static uint kDataFormatVersion = 2u;

	public static uint kBakedSurfaceDataMinVersion = 2u;

	public static int kInvalidNodesCount = -1;

	private GridNode[] nodes;

	[JsonMember]
	public Vector3 rotation;

	[JsonMember]
	public Vector3 center;

	[JsonMember]
	public bool snapCenterToWorldGrid;

	[JsonMember]
	public Vector2 unclampedSize;

	[JsonMember]
	public float nodeSize = 1f;

	[JsonMember]
	public GraphCollision collision;

	[JsonMember]
	public float maxClimb = 0.4f;

	[JsonMember]
	public float maxSlope = 90f;

	[JsonMember]
	public int erodeIterations;

	[JsonMember]
	public bool erosionUseTags;

	[JsonMember]
	public int erosionFirstTag = 1;

	[JsonMember]
	public NumNeighbours neighbours = NumNeighbours.Eight;

	[JsonMember]
	public bool cutCorners;

	[JsonMember]
	public float penaltyPositionOffset;

	[JsonMember]
	public bool penaltyPosition;

	[JsonMember]
	public float penaltyPositionFactor = 1f;

	[JsonMember]
	public bool penaltyAngle;

	[JsonMember]
	public float penaltyAngleFactor = 100f;

	[JsonMember]
	public float penaltyAnglePower = 1f;

	[JsonMember]
	public bool useJumpPointSearch;

	[JsonMember]
	public bool showMeshOutline = true;

	[JsonMember]
	public bool showNodeConnections;

	[JsonMember]
	public bool showMeshSurface = true;

	[JsonMember]
	public bool showNodeCoordinates;

	public override bool isScanned => true;

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes != null)
		{
			GridNode[] array = nodes;
			foreach (GridNode obj in array)
			{
				action(obj);
			}
		}
	}

	protected override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		int num2;
		if (num == kDataFormatSignature)
		{
			ctx.reader.ReadUInt32();
			num2 = ctx.reader.ReadInt32();
		}
		else
		{
			_ = kDataFormatVersionLegacy;
			num2 = num;
		}
		if (num2 >= 0)
		{
			nodes = new GridNode[num2];
			for (int i = 0; i < num2; i++)
			{
				GridNode gridNode = new GridNode(active);
				nodes[i] = gridNode;
			}
		}
		else
		{
			nodes = null;
		}
	}
}
