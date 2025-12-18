using System;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class TraverseData
{
	private readonly TraverseType TraverseType;

	public readonly GraphNode GraphNodeFrom;

	public readonly GraphNode GraphNodeTo;

	public readonly Vector3 WaypointFrom;

	public readonly Vector3 WaypointTo;

	public readonly bool IsUpTraverse;

	public readonly float TraverseHeight;

	public readonly float TraverseDistance;

	public readonly float GraphNodeFromY;

	public readonly float GraphNodeToY;

	public readonly Vector2 GraphNodeFrom2D;

	public readonly Vector2 GraphNodeTo2D;

	public readonly Vector2 WaypointFrom2D;

	public readonly Vector2 WaypointTo2D;

	public bool IsLeapTraverse => TraverseType == TraverseType.Leap;

	public bool IsLedgeTraverse => TraverseType == TraverseType.Ledge;

	public bool IsLadderTraverse => TraverseType == TraverseType.Ladder;

	public bool HasPathAfterTraverse { get; set; }

	public float SpeedBeforeTraverse { get; set; }

	public TraverseData(GraphNode graphNodeFrom, GraphNode graphNodeTo, Vector3 waypointFrom, Vector3 waypointTo)
	{
		GraphNodeFrom = graphNodeFrom;
		GraphNodeTo = graphNodeTo;
		WaypointFrom = waypointFrom;
		WaypointTo = waypointTo;
		Vector3 vector = WaypointTo - WaypointFrom;
		IsUpTraverse = vector.y > 0f;
		TraverseHeight = Math.Abs(vector.y);
		TraverseDistance = vector.magnitude;
		TraverseType = WarhammerNodeLink.GetTraverseType(GraphNodeFrom, GraphNodeTo);
		GraphNodeFromY = GraphNodeFrom.Vector3Position().y;
		GraphNodeToY = GraphNodeTo.Vector3Position().y;
		GraphNodeFrom2D = GraphNodeFrom.Vector3Position().To2D();
		GraphNodeTo2D = GraphNodeTo.Vector3Position().To2D();
		WaypointFrom2D = WaypointFrom.To2D();
		WaypointTo2D = WaypointTo.To2D();
	}
}
