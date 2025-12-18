using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Pooling;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerFunnelModifier : PathModifier
{
	private enum LinecastResult
	{
		Miss,
		Hit,
		BadHit
	}

	public float AgentRadius = 1f;

	public EntityRef<AbstractUnitEntity> Unit;

	private readonly float m_OffsetForLastNode = 0.4f;

	private static readonly Vector3[][] m_NeighboursRectOffset = new Vector3[8][]
	{
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2]
	};

	private static readonly Vector3[] m_OffsetDirection = new Vector3[8];

	public override int Order => 10;

	public bool DebugAmplify
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	private List<Vector3> ApplyOldFunnel(List<GraphNode> allPathNodes, Funnel.PathPart part)
	{
		WarhammerFunnelOld.FunnelPortals funnelPortals = WarhammerFunnelOld.ConstructFunnelPortals(allPathNodes, part);
		WarhammerFunnelOld.ShrinkPortals(funnelPortals, AgentRadius);
		List<Vector3> list = WarhammerFunnelOld.Calculate(funnelPortals, unwrap: true, splitAtEveryPortal: false);
		List<Vector3> list2 = new List<Vector3>();
		list2.AddRange(list);
		global::Pathfinding.Pooling.ListPool<Vector3>.Release(ref funnelPortals.left);
		global::Pathfinding.Pooling.ListPool<Vector3>.Release(ref funnelPortals.right);
		global::Pathfinding.Pooling.ListPool<Vector3>.Release(ref list);
		return list2;
	}

	public override void Apply(Path path)
	{
		List<Funnel.PathPart> list = Funnel.SplitIntoParts(path);
		List<Vector3> list2 = global::Pathfinding.Pooling.ListPool<Vector3>.Claim();
		List<GraphNode> list3 = global::Pathfinding.Pooling.ListPool<GraphNode>.Claim();
		Vector3 vector = Vector3.up * 0.5f;
		if (DebugAmplify)
		{
			for (int i = 1; i < path.vectorPath.Count; i++)
			{
				Debug.DrawLine(path.vectorPath[i - 1] + vector, path.vectorPath[i] + vector, Color.gray);
			}
		}
		try
		{
			for (int j = 0; j < list.Count; j++)
			{
				Funnel.PathPart part = list[j];
				if (part.type == Funnel.PartType.NodeSequence)
				{
					List<Vector3> list4 = ApplyOldFunnel(path.path, part);
					for (int k = 0; k < list4.Count; k++)
					{
						Vector3 item = list4[k];
						LinkNode item2 = null;
						if (k == 0 && j > 0 && list[j - 1].type == Funnel.PartType.OffMeshLink)
						{
							item2 = path.path[list[j - 1].endIndex] as LinkNode;
						}
						else if (k == list4.Count - 1 && j < list.Count - 1 && list[j + 1].type == Funnel.PartType.OffMeshLink)
						{
							item2 = path.path[list[j + 1].startIndex] as LinkNode;
						}
						list2.Add(item);
						list3.Add(item2);
					}
				}
				else
				{
					if (j == 0 || list[j - 1].type == Funnel.PartType.OffMeshLink)
					{
						Vector3 startPoint = part.startPoint;
						list2.Add(startPoint);
						list3.Add(path.path[j] as LinkNode);
					}
					if (j == list.Count - 1 || list[j + 1].type == Funnel.PartType.OffMeshLink)
					{
						Vector3 endPoint = part.endPoint;
						list2.Add(endPoint);
						list3.Add(path.path[j] as LinkNode);
					}
				}
			}
		}
		finally
		{
			global::Pathfinding.Pooling.ListPool<Vector3>.Release(path.vectorPath);
			path.vectorPath = list2;
			global::Pathfinding.Pooling.ListPool<GraphNode>.Release(path.path);
			if (list3.Count > 0)
			{
				if (path.path.Count > 0)
				{
					list3[0] = path.path[0];
					int index = list3.Count - 1;
					List<GraphNode> path2 = path.path;
					list3[index] = path2[path2.Count - 1];
				}
				if (list3[0] == null)
				{
					list3[0] = ObstacleAnalyzer.GetNearestNode(list2[0]).node;
				}
				if (list3[list3.Count - 1] == null)
				{
					list3[list3.Count - 1] = ObstacleAnalyzer.GetNearestNode(list2[list2.Count - 1]).node;
				}
			}
			path.path = list3;
			AdjustWaypoint(path, path.vectorPath.Count - 1);
		}
	}

	private void AdjustWaypoint(Path path, int index)
	{
		if (AstarPath.active == null)
		{
			return;
		}
		Vector3 vector = path.vectorPath[index];
		GridNode gridNode = ObstacleAnalyzer.GetNearestNode(vector).node as GridNode;
		GridGraph gridGraph = gridNode?.Graph as GridGraph;
		if (gridNode == null || gridGraph == null)
		{
			PFLog.Default.Warning("Can't calculate neighbours walkability for offset");
			return;
		}
		float num = gridGraph.nodeSize / 2f;
		Vector3 vector2 = (Vector3)gridNode.position;
		Vector3[][] neighboursRectOffset = m_NeighboursRectOffset;
		neighboursRectOffset[0][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[0][1] = new Vector3(num, 0f, 0f - num + m_OffsetForLastNode);
		neighboursRectOffset[1][0] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num);
		neighboursRectOffset[1][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[2][0] = new Vector3(0f - num, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[2][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[3][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[3][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, num);
		neighboursRectOffset[4][0] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num);
		neighboursRectOffset[4][1] = new Vector3(num, 0f, 0f - num + m_OffsetForLastNode);
		neighboursRectOffset[5][0] = new Vector3(num - m_OffsetForLastNode, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[5][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[6][0] = new Vector3(0f - num, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[6][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, num);
		neighboursRectOffset[7][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[7][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		Vector3[] offsetDirection = m_OffsetDirection;
		offsetDirection[0] = new Vector3(0f, 0f, 0f - num + m_OffsetForLastNode);
		offsetDirection[1] = new Vector3(num - m_OffsetForLastNode, 0f, 0f);
		offsetDirection[2] = new Vector3(0f, 0f, num - m_OffsetForLastNode);
		offsetDirection[3] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f);
		offsetDirection[4] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		offsetDirection[5] = new Vector3(num - m_OffsetForLastNode, 0f, num - m_OffsetForLastNode);
		offsetDirection[6] = new Vector3(0f - num + m_OffsetForLastNode, num - m_OffsetForLastNode);
		offsetDirection[7] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		Vector3 vector3 = vector - vector2;
		int num2 = 8;
		bool[] array = new bool[num2];
		for (int i = 0; i < num2; i++)
		{
			GridNodeBase gridNodeBase = gridGraph.nodes[gridNode.NodeInGridIndex + gridGraph.neighbourOffsets[i]];
			array[i] = gridNodeBase.Walkable;
		}
		for (int j = 0; j < num2; j++)
		{
			if (!array[j] && vector3.x >= neighboursRectOffset[j][0].x - 0.0001f && vector3.x <= neighboursRectOffset[j][1].x + 0.0001f && vector3.z >= neighboursRectOffset[j][0].z - 0.0001f && vector3.z <= neighboursRectOffset[j][1].z + 0.0001f)
			{
				if (offsetDirection[j].x != 0f)
				{
					vector3.x = offsetDirection[j].x;
				}
				if (offsetDirection[j].z != 0f)
				{
					vector3.z = offsetDirection[j].z;
				}
			}
		}
		path.vectorPath[index] = vector3 + vector2;
	}

	private bool NodesConnected(GridNodeBase a, GridNodeBase b)
	{
		for (int i = 0; i < 8; i++)
		{
			if (a.GetNeighbourAlongDirection(i) == b)
			{
				return true;
			}
		}
		return false;
	}

	private LinecastResult LinecastAndGetCornerOfIntersection(Vector3 from, Vector3 to, out Vector3 addedPoint, bool draw)
	{
		GridGraph gridGraph = AstarPath.active.data.gridGraph;
		Vector3 vector = Vector3.up * 0.5f;
		addedPoint = Vector3.zero;
		if (!gridGraph.Linecast(from, to, out GraphHitInfo hit, (List<GraphNode>)null, (Func<GraphNode, bool>)null) || hit.point == to)
		{
			return LinecastResult.Miss;
		}
		Vector3 vector2 = (Vector3)(hit.node as GridNode).position;
		float num = vector2.x - gridGraph.nodeSize / 2f;
		float num2 = vector2.x + gridGraph.nodeSize / 2f;
		float num3 = vector2.z - gridGraph.nodeSize / 2f;
		float num4 = vector2.z + gridGraph.nodeSize / 2f;
		Vector3 point = hit.point;
		if (DebugAmplify)
		{
			Debug.DrawLine(from + vector, point + vector, Color.cyan);
		}
		float x = ((point.x < vector2.x) ? num : num2);
		float z = ((point.z < vector2.z) ? num3 : num4);
		addedPoint = new Vector3(x, vector2.y, z);
		if ((addedPoint.ToXZ() - from.ToXZ()).sqrMagnitude <= 0.001f)
		{
			return LinecastResult.BadHit;
		}
		float num5 = from.y + (to.y - from.y) * hit.distance / (to - from).magnitude;
		if (Mathf.Abs(addedPoint.y - num5) > 1f)
		{
			addedPoint.y = num5;
		}
		if (draw)
		{
			if (DebugAmplify)
			{
				Debug.DrawLine(from + vector, addedPoint + vector, new Color(0f, 0.3f, 1f));
			}
			if (DebugAmplify)
			{
				Debug.DrawLine(addedPoint + vector, to, new Color(0f, 0.3f, 1f));
			}
		}
		return LinecastResult.Hit;
	}

	private void DrawDebugArrow(Vector3 start, Vector3 end, Color color, float duration = 0f)
	{
		Vector3 vector = Vector3.up * 0.5f;
		Debug.DrawLine(start + vector, end + vector, color, duration);
		Vector3 normalized = (end - start).normalized;
		float num = 0.1f;
		Quaternion quaternion = Quaternion.LookRotation(normalized);
		Vector3 vector2 = quaternion * Quaternion.Euler(0f, 225f, 0f) * Vector3.forward * num;
		Vector3 vector3 = quaternion * Quaternion.Euler(0f, 135f, 0f) * Vector3.forward * num;
		Debug.DrawLine(end + vector, end + vector2 + vector, color, duration);
		Debug.DrawLine(end + vector, end + vector3 + vector, color, duration);
	}

	private void ApplyAmplification(List<Vector3> path)
	{
		GridGraph gridGraph = AstarPath.active.data.gridGraph;
		Vector3 vector = Vector3.up * 0.5f;
		if (DebugAmplify)
		{
			for (int i = 1; i < path.Count; i++)
			{
				Debug.DrawLine(path[i - 1] + vector, path[i] + vector, Color.blue);
			}
		}
		for (int j = 1; j < path.Count - 1; j++)
		{
			Vector3 vector2 = path[j] - path[j - 1];
			vector2.y = 0f;
			if (vector2.sqrMagnitude < 1E-05f)
			{
				path.RemoveAt(j);
				j--;
			}
		}
		for (int k = 1; k < path.Count - 1; k++)
		{
			Vector3 vector3 = path[k] - path[k - 1];
			Vector3 normalized = (path[k] - path[k - 1]).normalized;
			if (vector3.x == 0f || vector3.z == 0f)
			{
				int num = 1;
				float num2 = vector3.magnitude;
				while (num2 > ObstacleAnalyzer.CellSize)
				{
					num2 -= ObstacleAnalyzer.CellSize;
					path.Insert(k, path[k] + normalized * ObstacleAnalyzer.CellSize * num++);
				}
			}
		}
		int num3 = 20;
		int num4 = 0;
		for (int l = 1; l < path.Count - 1; l++)
		{
			Vector3 from = path[l] - path[l - 1];
			Vector3 to = path[l + 1] - path[l];
			from.y = 0f;
			to.y = 0f;
			Vector3 vector4 = from.normalized - to.normalized;
			float num5 = AgentRadius;
			if (vector4.sqrMagnitude < 1E-05f)
			{
				vector4 = new Vector3(0f - from.z, 0f, from.x).normalized;
			}
			else
			{
				num5 /= Mathf.Sin(Vector3.SignedAngle(from, to, Vector3.up) / 2f * MathF.PI / 180f);
			}
			num5 = Mathf.Min(num5, ObstacleAnalyzer.CellSize - AgentRadius);
			Vector3 vector5 = vector4 * num5;
			Vector3 vector6 = path[l];
			GridNodeBase nearestNodeXZUnwalkable = vector6.GetNearestNodeXZUnwalkable();
			Vector3 newPoint = path[l] + vector5;
			Vector3 vector7 = path[l] - vector5;
			GridNodeBase nearestNodeXZUnwalkable2 = newPoint.GetNearestNodeXZUnwalkable();
			GridNodeBase nearestNodeXZUnwalkable3 = vector7.GetNearestNodeXZUnwalkable();
			if (nearestNodeXZUnwalkable == null)
			{
				PFLog.Pathfinding.Error("Path amplification failed: cannot find suitable node");
				return;
			}
			if (DebugAmplify)
			{
				Vector3 vector8 = (Vector3)nearestNodeXZUnwalkable.position;
				Debug.DrawLine(vector8 + new Vector3(-0.35f, 0f, -0.35f) + vector, vector8 + new Vector3(0.35f, 0f, 0.35f) + vector, Color.white);
				Debug.DrawLine(vector8 + new Vector3(0.35f, 0f, -0.35f) + vector, vector8 + new Vector3(-0.35f, 0f, 0.35f) + vector, Color.white);
			}
			if (DebugAmplify)
			{
				Vector3 vector9 = (Vector3)nearestNodeXZUnwalkable2.position;
				Debug.DrawLine(vector9 + new Vector3(-0.3f, 0f, -0.3f) + vector, vector9 + new Vector3(0.3f, 0f, 0.3f) + vector, Color.yellow);
				Debug.DrawLine(vector9 + new Vector3(0.3f, 0f, -0.3f) + vector, vector9 + new Vector3(-0.3f, 0f, 0.3f) + vector, Color.yellow);
			}
			if (nearestNodeXZUnwalkable2.Walkable != nearestNodeXZUnwalkable3.Walkable)
			{
				if (!nearestNodeXZUnwalkable2.Walkable)
				{
					newPoint = vector7;
				}
			}
			else
			{
				CheckPoint(gridGraph, vector6, newPoint, path[l + 1], nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
				bool flag = CheckPoint(gridGraph, vector6, vector7, path[l + 1], nearestNodeXZUnwalkable, nearestNodeXZUnwalkable3);
				Vector3 normalized2 = (path[l] - path[l - 1]).normalized;
				for (int m = 0; m < 100; m++)
				{
					bool flag2 = false;
					Vector3 vector10 = normalized2 * 0.01f * m;
					for (int n = -1; n <= 1; n += 2)
					{
						Vector3 vector11 = vector6 + vector10 * n;
						nearestNodeXZUnwalkable = vector11.GetNearestNodeXZUnwalkable();
						if (nearestNodeXZUnwalkable == null)
						{
							PFLog.Pathfinding.Error("Path amplification failed: cannot find suitable node during retrace");
							return;
						}
						newPoint = vector11 + vector5;
						vector7 = vector11 - vector5;
						nearestNodeXZUnwalkable2 = newPoint.GetNearestNodeXZUnwalkable();
						nearestNodeXZUnwalkable3 = vector7.GetNearestNodeXZUnwalkable();
						bool num6 = nearestNodeXZUnwalkable2.Walkable && CheckPoint(gridGraph, vector6, newPoint, path[l + 1], nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
						flag = nearestNodeXZUnwalkable3.Walkable && CheckPoint(gridGraph, vector6, vector7, path[l + 1], nearestNodeXZUnwalkable, nearestNodeXZUnwalkable3);
						if (num6 != flag)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				if (flag)
				{
					newPoint = vector7;
				}
			}
			if (DebugAmplify)
			{
				Debug.DrawLine(path[l] + vector, newPoint + vector, Color.red);
			}
			Vector3 addedPoint;
			switch (LinecastAndGetCornerOfIntersection(path[l - 1], newPoint, out addedPoint, draw: true))
			{
			case LinecastResult.Hit:
				if ((newPoint - path[l - 1]).sqrMagnitude <= 0.0001f)
				{
					PFLog.Pathfinding.Error("WTF");
					break;
				}
				if (num4 >= num3)
				{
					PFLog.Pathfinding.Error("Path cannot be adjusted by ApplyAmplification, because it needs too many insertations. Bugs will follow.");
					break;
				}
				path.Insert(l, addedPoint);
				l--;
				num4++;
				break;
			case LinecastResult.Miss:
				path[l] = newPoint;
				break;
			}
			bool CheckPoint(GridGraph gg, Vector3 prevPoint, Vector3 testPoint, Vector3 nextPoint, GridNodeBase prevNode, GridNodeBase testNode)
			{
				Vector3 normalized3 = (nextPoint - newPoint).normalized;
				GraphHitInfo hit;
				bool num8 = gg.Linecast(prevPoint, testPoint, out hit, (List<GraphNode>)null, (Func<GraphNode, bool>)null) && hit.point != testPoint;
				GraphHitInfo hit2;
				bool flag3 = gg.Linecast((Vector3)prevNode.position, (Vector3)testNode.position, out hit2, (List<GraphNode>)null, (Func<GraphNode, bool>)null) && hit2.point != (Vector3)testNode.position;
				Vector3 vector12 = testPoint + normalized3 * ObstacleAnalyzer.CellSize * Mathf.Sqrt(2f);
				GraphHitInfo hit3;
				bool flag4 = gg.Linecast(testPoint, vector12, out hit3, (List<GraphNode>)null, (Func<GraphNode, bool>)null) && hit3.point != vector12;
				return !(num8 || flag4 || flag3);
			}
		}
		if (DebugAmplify)
		{
			for (int num7 = 1; num7 < path.Count; num7++)
			{
				Debug.DrawLine(path[num7 - 1] + vector, path[num7] + vector, Color.green);
			}
		}
		if (DebugAmplify)
		{
			Debug.Break();
		}
	}
}
