using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class Linecast
{
	public interface ICanTransitionBetweenCells
	{
		bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct EmptyTransition : ICanTransitionBetweenCells
	{
		public static EmptyTransition Instance;

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			return true;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct HasConnectionTransition : ICanTransitionBetweenCells
	{
		public static HasConnectionTransition Instance;

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			return nodeFrom.ContainsConnection(nodeTo);
		}
	}

	public readonly struct Ray2NodeOffsets : IEnumerable<Vector2Int>, IEnumerable
	{
		public struct Enumerator : IEnumerator<Vector2Int>, IEnumerator, IDisposable
		{
			private readonly Vector2Int m_Start;

			private readonly Vector2Int m_MaxDeltaInc;

			private readonly Vector2Int m_MinDeltaInc;

			private readonly float m_MaxDelta;

			private readonly float m_MinDelta;

			private float m_Error;

			private Vector2Int m_NextNode;

			private Vector2Int m_CurrentNode;

			public Vector2Int Current => m_CurrentNode;

			object IEnumerator.Current => Current;

			public Enumerator(Vector2Int start, Vector2 direction)
			{
				m_Start = start;
				int x = Math.Sign(direction.x);
				int y = Math.Sign(direction.y);
				float num = Math.Abs(direction.x);
				float num2 = Math.Abs(direction.y);
				bool flag = num > num2;
				m_MaxDeltaInc = (flag ? new Vector2Int(x, 0) : new Vector2Int(0, y));
				m_MinDeltaInc = ((!flag) ? new Vector2Int(x, 0) : new Vector2Int(0, y));
				m_MaxDelta = (flag ? num : num2);
				m_MinDelta = ((!flag) ? num : num2);
				m_Error = m_MinDelta;
				m_NextNode = start;
				m_CurrentNode = new Vector2Int(int.MinValue, int.MinValue);
			}

			public bool MoveNext()
			{
				m_CurrentNode = m_NextNode;
				m_NextNode += m_MaxDeltaInc;
				m_Error += m_MinDelta;
				if (m_Error >= m_MaxDelta)
				{
					m_NextNode += m_MinDeltaInc;
					m_Error -= m_MaxDelta;
				}
				return true;
			}

			public void Reset()
			{
				m_Error = m_MinDelta;
				m_NextNode = m_Start;
				m_CurrentNode = new Vector2Int(int.MinValue, int.MinValue);
			}

			public void Dispose()
			{
			}
		}

		private readonly Vector2Int m_Start;

		private readonly Vector2 m_Direction;

		public Ray2NodeOffsets(Vector2Int start, Vector2 direction)
		{
			m_Start = start;
			m_Direction = direction;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(m_Start, m_Direction);
		}

		IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public readonly struct Ray2Nodes : IEnumerable<GridNodeBase>, IEnumerable
	{
		public struct Enumerator : IEnumerator<GridNodeBase>, IEnumerator, IDisposable
		{
			private readonly GridGraph m_Graph;

			private Ray2NodeOffsets.Enumerator m_Offsets;

			public GridNodeBase Current => m_Graph.GetNode(m_Offsets.Current.x, m_Offsets.Current.y);

			object IEnumerator.Current => Current;

			public Enumerator(GridGraph graph, Ray2NodeOffsets.Enumerator offsets)
			{
				m_Graph = graph;
				m_Offsets = offsets;
			}

			public bool MoveNext()
			{
				if (m_Offsets.MoveNext())
				{
					return Current != null;
				}
				return false;
			}

			public void Reset()
			{
				m_Offsets.Reset();
			}

			public void Dispose()
			{
				m_Offsets.Reset();
			}
		}

		private readonly GridGraph m_Graph;

		private readonly Ray2NodeOffsets m_Offsets;

		public Ray2Nodes(GridGraph graph, in Ray2NodeOffsets offsets)
		{
			m_Graph = graph;
			m_Offsets = offsets;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(m_Graph, m_Offsets.GetEnumerator());
		}

		IEnumerator<GridNodeBase> IEnumerable<GridNodeBase>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public struct GraphHitInfo
	{
		public Vector3 origin;

		public Vector3 point;

		public GridNodeBase node;

		public GridNodeBase nextNode;

		public Vector3 tangentOrigin;

		public Vector3 tangent;

		public readonly float distance => (point - origin).magnitude;
	}

	private static readonly NNConstraint DefaultConstraint = NNConstraint.None;

	public static bool LinecastGrid(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
	{
		return LinecastGrid(graph, origin, end, hint, out hit, DefaultConstraint, ref EmptyTransition.Instance);
	}

	public static bool LinecastGrid(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, NNConstraint constraint)
	{
		return LinecastGrid(graph, origin, end, hint, out hit, constraint, ref EmptyTransition.Instance);
	}

	public static bool LinecastGrid<T>(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, ref T condition) where T : ICanTransitionBetweenCells
	{
		return LinecastGrid(graph, origin, end, hint, out hit, DefaultConstraint, ref condition);
	}

	public static bool LinecastGrid<T>(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, NNConstraint constraint, ref T condition, float precision = 0.01f) where T : ICanTransitionBetweenCells
	{
		hit = default(GraphHitInfo);
		if (float.IsNaN(origin.x + origin.y + origin.z))
		{
			throw new ArgumentException("origin is NaN");
		}
		if (float.IsNaN(end.x + end.y + end.z))
		{
			throw new ArgumentException("end is NaN");
		}
		GraphTransform transform = (graph as ITransformedGraph).transform;
		GridNodeBase gridNodeBase = (hint as GridNodeBase) ?? (graph.GetNearest(origin, constraint).node as GridNodeBase);
		if (gridNodeBase == null)
		{
			Debug.LogError("Could not find a valid node to start from");
			hit.origin = origin;
			hit.point = origin;
			return true;
		}
		Vector3 vector = ClosestPointOnGridNodeInGraphSpace(gridNodeBase, origin, end);
		hit.origin = transform.Transform(vector);
		Vector3 vector2 = transform.InverseTransform(end);
		if (vector == vector2)
		{
			hit.point = hit.origin;
			hit.node = gridNodeBase;
			return false;
		}
		Vector3 vector3 = vector2 - vector;
		Vector3 vector4 = new Vector3((vector3.x > 0f) ? 0.5f : (-0.5f), 0f, (vector3.z > 0f) ? 0.5f : (-0.5f));
		(Vector3, Vector3) tuple = (new Vector3(vector4.x, 0f, 0f - vector4.z), new Vector3(vector4.x, 0f, vector4.z));
		(Vector3, Vector3) tuple2 = (new Vector3(0f - vector4.x, 0f, vector4.z), new Vector3(vector4.x, 0f, vector4.z));
		int num = ((vector3.x > 0f) ? 1 : 3);
		int num2 = ((vector3.z > 0f) ? 2 : 0);
		int num3 = ((!(vector3.x >= 0f)) ? ((vector3.z >= 0f) ? 6 : 7) : ((vector3.z >= 0f) ? 5 : 4));
		int num4 = 0;
		Vector3 point;
		GridNodeBase neighbourAlongDirection;
		while (true)
		{
			num4++;
			if (num4 > 2000)
			{
				Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
				hit.point = hit.origin;
				return true;
			}
			Vector3 vector5 = transform.InverseTransform((Vector3)gridNodeBase.position);
			float factor;
			float factor2;
			bool flag = VectorMath.LineIntersectionFactorXZ(vector, vector2, vector5 + tuple.Item1, vector5 + tuple.Item2, out factor, out factor2);
			float factor3;
			float factor4;
			bool flag2 = VectorMath.LineIntersectionFactorXZ(vector, vector2, vector5 + tuple2.Item1, vector5 + tuple2.Item2, out factor3, out factor4);
			Vector3 vector6 = vector + factor * (vector2 - vector);
			Vector3 vector7 = vector + factor3 * (vector2 - vector);
			if ((!flag || factor >= 1f) && (!flag2 || factor3 >= 1f))
			{
				hit.point = transform.Transform(vector2);
				hit.node = gridNodeBase;
				return false;
			}
			int direction;
			float num5;
			if (flag && !flag2)
			{
				direction = num;
				point = vector6;
				num5 = factor;
			}
			else if (!flag && flag2)
			{
				direction = num2;
				point = vector7;
				num5 = factor3;
			}
			else if (Math.Abs(factor - factor3) < precision)
			{
				direction = num3;
				point = ((factor < factor3) ? vector6 : vector7);
				num5 = Math.Min(factor, factor3);
			}
			else if (factor < factor3)
			{
				direction = num;
				point = vector6;
				num5 = factor;
			}
			else
			{
				direction = num2;
				point = vector7;
				num5 = factor3;
			}
			Vector3 transitionPosition = Vector3.Lerp(origin, end, num5);
			neighbourAlongDirection = gridNodeBase.GetNeighbourAlongDirection(direction, checkConnectivity: false);
			if (neighbourAlongDirection == null || (constraint != null && !constraint.Suitable(neighbourAlongDirection)) || !condition.CanTransitionBetweenCells(gridNodeBase, neighbourAlongDirection, transitionPosition, num5))
			{
				break;
			}
			gridNodeBase = neighbourAlongDirection;
		}
		hit.point = transform.Transform(point);
		Vector3 vector8 = hit.point - hit.origin;
		float magnitude = vector8.magnitude;
		if (magnitude == 0f)
		{
			vector8 = hit.point - gridNodeBase.Vector3Position();
			magnitude = vector8.magnitude;
		}
		Vector3 vector9 = vector8 / magnitude;
		hit.point = hit.origin + vector9 * (magnitude - 0.0001f);
		hit.node = gridNodeBase;
		hit.nextNode = neighbourAlongDirection;
		return true;
	}

	public static bool LinecastGrid2<T>(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, NNConstraint constraint, ref T condition) where T : ICanTransitionBetweenCells
	{
		hit = default(GraphHitInfo);
		if (float.IsNaN(origin.x + origin.y + origin.z))
		{
			throw new ArgumentException("origin is NaN");
		}
		if (float.IsNaN(end.x + end.y + end.z))
		{
			throw new ArgumentException("end is NaN");
		}
		GraphTransform transform = (graph as ITransformedGraph).transform;
		GridNodeBase gridNodeBase = (hint as GridNodeBase) ?? (graph.GetNearest(origin, constraint).node as GridNodeBase);
		GridGraph graph2 = (GridGraph)graph;
		if (gridNodeBase == null)
		{
			Debug.LogError("Could not find a valid node to start from");
			hit.origin = origin;
			hit.point = origin;
			return true;
		}
		Vector3 vector = ClosestPointOnGridNodeInGraphSpace(gridNodeBase, origin, end);
		hit.origin = transform.Transform(vector);
		Vector3 vector2 = transform.InverseTransform(end);
		if (vector == vector2)
		{
			hit.point = hit.origin;
			hit.node = gridNodeBase;
			return false;
		}
		Vector2 direction = (vector2 - vector).normalized.To2D();
		float magnitude = (end - origin).To2D().magnitude;
		Ray2NodeOffsets offsets = new Ray2NodeOffsets(gridNodeBase.CoordinatesInGrid, direction);
		using Ray2Nodes.Enumerator enumerator = new Ray2Nodes(graph2, in offsets).GetEnumerator();
		enumerator.MoveNext();
		if (enumerator.Current != gridNodeBase)
		{
			throw new Exception("Internal Linecast error");
		}
		while (enumerator.MoveNext())
		{
			GridNodeBase current = enumerator.Current;
			Vector3 vector3 = ((current != null) ? ((current.Vector3Position() + gridNodeBase.Vector3Position()) / 2f) : gridNodeBase.Vector3Position());
			float num = (vector3 - origin).To2D().magnitude / magnitude;
			if (current == null || num > 1f || (constraint != null && !constraint.Suitable(current)) || !condition.CanTransitionBetweenCells(gridNodeBase, current, vector3, num))
			{
				hit.point = vector3;
				hit.node = gridNodeBase;
				return true;
			}
			gridNodeBase = current;
		}
		return true;
	}

	private static bool LinecastGrid3<T>(NavGraph graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, NNConstraint constraint, ref T condition) where T : ICanTransitionBetweenCells
	{
		hit = default(GraphHitInfo);
		if (float.IsNaN(origin.x + origin.y + origin.z))
		{
			throw new ArgumentException("origin is NaN");
		}
		if (float.IsNaN(end.x + end.y + end.z))
		{
			throw new ArgumentException("end is NaN");
		}
		GraphTransform transform = (graph as ITransformedGraph).transform;
		GridNodeBase gridNodeBase = (hint as GridNodeBase) ?? (graph.GetNearest(origin, constraint).node as GridNodeBase);
		GridGraph gridGraph = (GridGraph)graph;
		if (gridNodeBase == null)
		{
			Debug.LogError("Could not find a valid node to start from");
			hit.origin = origin;
			hit.point = origin;
			return true;
		}
		Vector3 vector = ClosestPointOnGridNodeInGraphSpace(gridNodeBase, origin, end);
		hit.origin = transform.Transform(vector);
		Vector3 vector2 = transform.InverseTransform(end);
		if (vector == vector2)
		{
			hit.point = hit.origin;
			hit.node = gridNodeBase;
			return false;
		}
		Vector2 vector3 = (vector2 - vector).normalized.To2D();
		int x = Math.Sign(vector3.x);
		int y = Math.Sign(vector3.y);
		float num = Math.Abs(vector3.x);
		float num2 = Math.Abs(vector3.y);
		float num3 = 0f;
		bool num4 = num > num2;
		Vector2Int vector2Int = (num4 ? new Vector2Int(x, 0) : new Vector2Int(0, y));
		Vector2Int vector2Int2 = ((!num4) ? new Vector2Int(x, 0) : new Vector2Int(0, y));
		float num5 = (num4 ? num : num2);
		float num6 = ((!num4) ? num : num2);
		num3 += num6;
		float magnitude = (end - origin).To2D().magnitude;
		Vector2Int vector2Int3 = new Vector2Int(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid);
		int num7 = 0;
		Vector3 vector4;
		while (true)
		{
			num7++;
			if (num7 > 2000)
			{
				Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
				hit.point = hit.origin;
				return true;
			}
			Vector2Int vector2Int4 = vector2Int3;
			vector2Int4 += vector2Int;
			num3 += num6;
			if (num3 >= num5)
			{
				vector2Int4 += vector2Int2;
				num3 -= num5;
			}
			GridNodeBase node = gridGraph.GetNode(vector2Int4.x, vector2Int4.y);
			vector4 = ((node != null) ? ((node.Vector3Position() + gridNodeBase.Vector3Position()) / 2f) : gridNodeBase.Vector3Position());
			float num8 = (vector4 - origin).To2D().magnitude / magnitude;
			if (node == null || num8 > 1f || (constraint != null && !constraint.Suitable(node)) || !condition.CanTransitionBetweenCells(gridNodeBase, node, vector4, num8))
			{
				break;
			}
			gridNodeBase = node;
			vector2Int3 = vector2Int4;
		}
		hit.point = vector4;
		hit.node = gridNodeBase;
		return true;
	}

	private static Vector3 ClosestPointOnGridNodeInGraphSpace(GridNodeBase node, Vector3 origin, Vector3 end)
	{
		GridGraph gridGraph = GridNode.GetGridGraph(node.GraphIndex);
		origin = gridGraph.transform.InverseTransform(origin);
		int num = node.NodeInGridIndex % gridGraph.width;
		int num2 = node.NodeInGridIndex / gridGraph.width;
		float y = gridGraph.transform.InverseTransform((Vector3)node.position).y;
		return new Vector3(Mathf.Clamp(origin.x, num, (float)num + 1f), y, Mathf.Clamp(origin.z, num2, (float)num2 + 1f));
	}
}
