using System;
using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Pathfinding.Pooling;
using UnityEngine;

namespace Pathfinding;

public class WarhammerFunnelOld
{
	public struct FunnelPortals
	{
		public List<Vector3> left;

		public List<Vector3> right;
	}

	public readonly struct DiagonalPortalInfo
	{
		public readonly Vector3 Middle;

		public readonly Vector3 Cross;

		public readonly bool LeftClear;

		public readonly bool RightClear;

		public DiagonalPortalInfo(Vector3 middle, Vector3 cross, bool leftClear, bool rightClear)
		{
			Middle = middle;
			Cross = cross;
			LeftClear = leftClear;
			RightClear = rightClear;
		}
	}

	public static List<Funnel.PathPart> SplitIntoParts(Path path)
	{
		List<GraphNode> path2 = path.path;
		List<Funnel.PathPart> list = ListPool<Funnel.PathPart>.Claim();
		if (path2 == null || path2.Count == 0)
		{
			return list;
		}
		bool flag = false;
		int num;
		for (num = 0; num < path2.Count; num++)
		{
			if (path2[num] is TriangleMeshNode || path2[num] is GridNodeBase)
			{
				Funnel.PathPart item = default(Funnel.PathPart);
				item.startIndex = num;
				uint graphIndex = path2[num].GraphIndex;
				bool flag2 = false;
				for (num++; num < path2.Count; num++)
				{
					if (path2[num].GraphIndex != graphIndex && !(path2[num] is NodeLink3Node))
					{
						break;
					}
					if (!(path2[num - 1] is GridNode gridNode))
					{
						continue;
					}
					GraphNode graphNode = path2[num];
					GridNodeBase gridNodeB = graphNode as GridNodeBase;
					if (gridNodeB != null)
					{
						bool hasConnection = false;
						gridNode.GetConnections(delegate(GraphNode c)
						{
							hasConnection = hasConnection || c == gridNodeB;
						});
						if (hasConnection)
						{
							flag2 = true;
							break;
						}
					}
				}
				num = (item.endIndex = num - 1);
				if (item.startIndex == 0)
				{
					item.startPoint = path.vectorPath[0];
				}
				else
				{
					int num2 = item.startIndex;
					if (!flag)
					{
						num2--;
					}
					item.startPoint = (Vector3)path2[num2].position;
				}
				if (item.endIndex == path2.Count - 1)
				{
					item.endPoint = path.vectorPath[path.vectorPath.Count - 1];
				}
				else
				{
					int num3 = item.endIndex;
					if (!flag2)
					{
						num3++;
					}
					item.endPoint = (Vector3)path2[num3].position;
				}
				flag = flag2;
				list.Add(item);
			}
			else
			{
				if (!path2[num].IsNodeLinked())
				{
					throw new Exception("Unsupported node type or null node");
				}
				Funnel.PathPart item2 = default(Funnel.PathPart);
				item2.startIndex = num;
				uint graphIndex2 = path2[num].GraphIndex;
				for (num++; num < path2.Count && path2[num].GraphIndex == graphIndex2; num++)
				{
				}
				num--;
				if (num - item2.startIndex != 0)
				{
					if (num - item2.startIndex != 1)
					{
						throw new Exception("NodeLink2 link length greater than two (2) nodes. " + (num - item2.startIndex + 1));
					}
					item2.endIndex = num;
					item2.type = Funnel.PartType.OffMeshLink;
					item2.startPoint = (Vector3)path2[item2.startIndex].position;
					item2.endPoint = (Vector3)path2[item2.endIndex].position;
					flag = false;
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public static DiagonalPortalInfo? GetDiagonalPortalRich(GridNode from, GraphNode other, bool checkHasConnection)
	{
		GridGraph gridGraph = other.Graph as GridGraph;
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNodeBase[] nodes = gridGraph.nodes;
		for (int i = 4; i < 8; i++)
		{
			if ((!from.HasConnectionInDirection(i) && checkHasConnection) || from.NodeInGridIndex + neighbourOffsets[i] >= gridGraph.nodes.Length || other != gridGraph.nodes[from.NodeInGridIndex + neighbourOffsets[i]])
			{
				continue;
			}
			bool rightClear = false;
			bool leftClear = false;
			if (from.HasConnectionInDirection(i - 4))
			{
				GridNodeBase gridNodeBase = nodes[from.NodeInGridIndex + neighbourOffsets[i - 4]];
				if (gridNodeBase.Walkable && gridNodeBase.HasConnectionInDirection((i - 4 + 1) % 4))
				{
					rightClear = true;
				}
			}
			if (from.HasConnectionInDirection((i - 4 + 1) % 4))
			{
				GridNodeBase gridNodeBase2 = nodes[from.NodeInGridIndex + neighbourOffsets[(i - 4 + 1) % 4]];
				if (gridNodeBase2.Walkable && gridNodeBase2.HasConnectionInDirection(i - 4))
				{
					leftClear = true;
				}
			}
			Vector3 middle = (Vector3)(from.position + other.position) * 0.5f;
			Vector3 cross = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - from.position));
			cross.Normalize();
			cross *= gridGraph.nodeSize * 1.4142f;
			return new DiagonalPortalInfo(middle, cross, leftClear, rightClear);
		}
		return null;
	}

	public static FunnelPortals ConstructFunnelPortals(List<GraphNode> nodes, Funnel.PathPart part)
	{
		FunnelPortals result;
		if (nodes == null || nodes.Count == 0)
		{
			result = default(FunnelPortals);
			result.left = ListPool<Vector3>.Claim(0);
			result.right = ListPool<Vector3>.Claim(0);
			return result;
		}
		if (part.endIndex < part.startIndex || part.startIndex < 0 || part.endIndex > nodes.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		List<Vector3> list = ListPool<Vector3>.Claim(nodes.Count + 1);
		List<Vector3> list2 = ListPool<Vector3>.Claim(nodes.Count + 1);
		list.Add(part.startPoint);
		list2.Add(part.startPoint);
		for (int i = part.startIndex; i < part.endIndex; i++)
		{
			if (nodes[i] is GridNode && i > part.startIndex)
			{
				Vector2Int coordinatesInGrid = ((GridNode)nodes[i - 1]).CoordinatesInGrid;
				Vector2Int coordinatesInGrid2 = ((GridNode)nodes[i]).CoordinatesInGrid;
				Vector2Int coordinatesInGrid3 = ((GridNode)nodes[i + 1]).CoordinatesInGrid;
				Vector2Int vector2Int = coordinatesInGrid - coordinatesInGrid2;
				Vector2Int vector2Int2 = coordinatesInGrid3 - coordinatesInGrid2;
				if (vector2Int.x + vector2Int2.x == 1 && vector2Int2.y + vector2Int.y == 1)
				{
					DiagonalPortalInfo? diagonalPortalRich = GetDiagonalPortalRich((GridNode)nodes[i - 1], nodes[i + 1], checkHasConnection: false);
					if (diagonalPortalRich.HasValue)
					{
						DiagonalPortalInfo valueOrDefault = diagonalPortalRich.GetValueOrDefault();
						list.Add(valueOrDefault.Middle - (valueOrDefault.LeftClear ? valueOrDefault.Cross : Vector3.zero));
						list2.Add(valueOrDefault.Middle + (valueOrDefault.RightClear ? valueOrDefault.Cross : Vector3.zero));
					}
				}
			}
			if (!nodes[i].GetPortal(nodes[i + 1], list, list2, backwards: false))
			{
				list.Add((Vector3)nodes[i].position);
				list2.Add((Vector3)nodes[i].position);
				list.Add((Vector3)nodes[i + 1].position);
				list2.Add((Vector3)nodes[i + 1].position);
			}
		}
		list.Add(part.endPoint);
		list2.Add(part.endPoint);
		result = default(FunnelPortals);
		result.left = list;
		result.right = list2;
		return result;
	}

	public static void ShrinkPortals(FunnelPortals portals, float shrink)
	{
		if (shrink <= 1E-05f)
		{
			return;
		}
		for (int i = 0; i < portals.left.Count; i++)
		{
			Vector3 vector = portals.left[i];
			Vector3 vector2 = portals.right[i];
			float magnitude = (vector - vector2).magnitude;
			if (magnitude > 0f)
			{
				float num = Mathf.Min(shrink / magnitude, 0.5f);
				portals.left[i] = Vector3.Lerp(vector, vector2, num);
				portals.right[i] = Vector3.Lerp(vector, vector2, 1f - num);
			}
		}
	}

	private static bool UnwrapHelper(Vector3 portalStart, Vector3 portalEnd, Vector3 prevPoint, Vector3 nextPoint, ref Quaternion mRot, ref Vector3 mOffset)
	{
		if (VectorMath.IsColinear(portalStart, portalEnd, nextPoint))
		{
			return false;
		}
		Vector3 vector = portalEnd - portalStart;
		float sqrMagnitude = vector.sqrMagnitude;
		prevPoint -= Vector3.Dot(prevPoint - portalStart, vector) / sqrMagnitude * vector;
		nextPoint -= Vector3.Dot(nextPoint - portalStart, vector) / sqrMagnitude * vector;
		Quaternion quaternion = Quaternion.FromToRotation(nextPoint - portalStart, portalStart - prevPoint);
		mOffset += mRot * (portalStart - quaternion * portalStart);
		mRot *= quaternion;
		return true;
	}

	public static void Unwrap(FunnelPortals funnel, Vector2[] left, Vector2[] right)
	{
		int num = 1;
		Vector3 fromDirection = Vector3.Cross(funnel.right[1] - funnel.left[0], funnel.left[1] - funnel.left[0]);
		while (fromDirection.sqrMagnitude <= 1E-08f && num + 1 < funnel.left.Count)
		{
			num++;
			fromDirection = Vector3.Cross(funnel.right[num] - funnel.left[0], funnel.left[num] - funnel.left[0]);
		}
		left[0] = (right[0] = Vector2.zero);
		Vector3 vector = funnel.left[1];
		Vector3 vector2 = funnel.right[1];
		Vector3 prevPoint = funnel.left[0];
		Quaternion mRot = Quaternion.FromToRotation(fromDirection, Vector3.forward);
		Vector3 mOffset = mRot * -funnel.right[0];
		for (int i = 1; i < funnel.left.Count; i++)
		{
			if (UnwrapHelper(vector, vector2, prevPoint, funnel.left[i], ref mRot, ref mOffset))
			{
				prevPoint = vector;
				vector = funnel.left[i];
			}
			left[i] = mRot * funnel.left[i] + mOffset;
			if (UnwrapHelper(vector, vector2, prevPoint, funnel.right[i], ref mRot, ref mOffset))
			{
				prevPoint = vector2;
				vector2 = funnel.right[i];
			}
			right[i] = mRot * funnel.right[i] + mOffset;
		}
	}

	private static int FixFunnel(Vector2[] left, Vector2[] right, int numPortals)
	{
		if (numPortals > left.Length || numPortals > right.Length)
		{
			throw new ArgumentException("Arrays do not have as many elements as specified");
		}
		if (numPortals < 3)
		{
			return -1;
		}
		int num = 0;
		while (left[num + 1] == left[num + 2] && right[num + 1] == right[num + 2])
		{
			left[num + 1] = left[num];
			right[num + 1] = right[num];
			num++;
			if (numPortals - num < 3)
			{
				return -1;
			}
		}
		return num;
	}

	protected static Vector2 ToXZ(Vector3 p)
	{
		return new Vector2(p.x, p.z);
	}

	protected static Vector3 FromXZ(Vector2 p)
	{
		return new Vector3(p.x, 0f, p.y);
	}

	protected static bool RightOrColinear(Vector2 a, Vector2 b)
	{
		return a.x * b.y - b.x * a.y <= 0f;
	}

	protected static bool LeftOrColinear(Vector2 a, Vector2 b)
	{
		return a.x * b.y - b.x * a.y >= 0f;
	}

	public static List<Vector3> Calculate(FunnelPortals funnel, bool unwrap, bool splitAtEveryPortal)
	{
		if (funnel.left.Count != funnel.right.Count)
		{
			throw new ArgumentException("funnel.left.Count != funnel.right.Count");
		}
		Vector2[] array = ArrayPool<Vector2>.Claim(funnel.left.Count);
		Vector2[] array2 = ArrayPool<Vector2>.Claim(funnel.left.Count);
		if (unwrap)
		{
			Unwrap(funnel, array, array2);
		}
		else
		{
			for (int i = 0; i < funnel.left.Count; i++)
			{
				array[i] = ToXZ(funnel.left[i]);
				array2[i] = ToXZ(funnel.right[i]);
			}
		}
		int num = FixFunnel(array, array2, funnel.left.Count);
		List<int> list = ListPool<int>.Claim();
		if (num == -1)
		{
			list.Add(0);
			list.Add(funnel.left.Count - 1);
		}
		else
		{
			Calculate(array, array2, funnel.left.Count, num, list, int.MaxValue, out var _);
		}
		List<Vector3> list2 = ListPool<Vector3>.Claim(list.Count);
		Vector2 p = array[0];
		int num2 = 0;
		for (int j = 0; j < list.Count; j++)
		{
			int num3 = list[j];
			if (splitAtEveryPortal)
			{
				Vector2 vector = ((num3 >= 0) ? array[num3] : array2[-num3]);
				for (int k = num2 + 1; k < Math.Abs(num3); k++)
				{
					float t = VectorMath.LineIntersectionFactorXZ(FromXZ(array[k]), FromXZ(array2[k]), FromXZ(p), FromXZ(vector));
					list2.Add(Vector3.Lerp(funnel.left[k], funnel.right[k], t));
				}
				num2 = Mathf.Abs(num3);
				p = vector;
			}
			if (num3 >= 0)
			{
				list2.Add(funnel.left[num3]);
			}
			else
			{
				list2.Add(funnel.right[-num3]);
			}
		}
		ListPool<int>.Release(ref list);
		ArrayPool<Vector2>.Release(ref array);
		ArrayPool<Vector2>.Release(ref array2);
		return list2;
	}

	private static void Calculate(Vector2[] left, Vector2[] right, int numPortals, int startIndex, List<int> funnelPath, int maxCorners, out bool lastCorner)
	{
		if (left.Length != right.Length)
		{
			throw new ArgumentException();
		}
		lastCorner = false;
		int num = startIndex;
		int num2 = startIndex + 1;
		int num3 = startIndex + 1;
		Vector2 vector = left[num];
		Vector2 vector2 = left[num3];
		Vector2 vector3 = right[num2];
		funnelPath.Add(num);
		for (int i = startIndex + 2; i < numPortals; i++)
		{
			if (funnelPath.Count >= maxCorners)
			{
				return;
			}
			if (funnelPath.Count > 2000)
			{
				Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
				break;
			}
			Vector2 vector4 = left[i];
			Vector2 vector5 = right[i];
			if (LeftOrColinear(vector3 - vector, vector5 - vector))
			{
				if (!(vector == vector3) && !RightOrColinear(vector2 - vector, vector5 - vector))
				{
					vector = (vector3 = vector2);
					i = (num = (num2 = num3));
					funnelPath.Add(num);
					continue;
				}
				vector3 = vector5;
				num2 = i;
			}
			if (RightOrColinear(vector2 - vector, vector4 - vector))
			{
				if (vector == vector2 || LeftOrColinear(vector3 - vector, vector4 - vector))
				{
					vector2 = vector4;
					num3 = i;
				}
				else
				{
					vector = (vector2 = vector3);
					i = (num = (num3 = num2));
					funnelPath.Add(-num);
				}
			}
		}
		lastCorner = true;
		funnelPath.Add(numPortals - 1);
	}
}
