using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public static class ObstaclePathfinder
{
	private const float MinPathLength = 1f;

	private const float ComfortCollisionDistance = 1f;

	private static UnitMovementAgent s_Unit;

	private static IReadOnlyList<UnitMovementAgent> s_Group;

	private static List<Vector3> s_Points;

	private static readonly List<Vector3> s_ProcessedPath = new List<Vector3>();

	private static readonly List<UnitMovementAgent> s_IgnoredObstacles = new List<UnitMovementAgent>();

	private static readonly ConvexHullStack s_LeftStack = new ConvexHullStack(1);

	private static readonly ConvexHullStack s_RightStack = new ConvexHullStack(-1);

	public static ObstaclePathingResult PathAroundStandingObstacles([NotNull] Path path, [NotNull] UnitMovementAgent unit, [CanBeNull] UnitMovementAgent target)
	{
		if (path.error || path.vectorPath.Count <= 0)
		{
			return ObstaclePathingResult.PathClear;
		}
		if (unit.AvoidanceDisabled)
		{
			return ObstaclePathingResult.PathClear;
		}
		try
		{
			s_ProcessedPath.Clear();
			s_Unit = unit;
			s_Points = path.vectorPath;
			s_ProcessedPath.Add(s_Points[0]);
			s_IgnoredObstacles.Clear();
			s_IgnoredObstacles.Add(unit);
			if (target != null)
			{
				s_IgnoredObstacles.Add(target);
			}
			Vector3 s = s_Points[0];
			List<UnitMovementAgent> allAgents = UnitMovementAgent.AllAgents;
			for (int i = 1; i < s_Points.Count; i++)
			{
				IntersectionResult intersection = IntersectAllObstacles(allAgents, s_Points[i - 1], s_Points[i]);
				if (!intersection.HasIntersection)
				{
					s = s_Points[i - 1];
					s_ProcessedPath.Add(s_Points[i]);
					continue;
				}
				float num = 1f + s_Unit.Corpulence;
				if (intersection.SqrDistance > num * num)
				{
					s = s_Points[i - 1];
				}
				Vector3 vector = s_Points[i];
				if ((intersection.Point - vector).sqrMagnitude < num * num)
				{
					vector = s_Points[Math.Min(i + 1, s_Points.Count - 1)];
				}
				if (DoPathing(intersection, s, vector))
				{
					s_ProcessedPath.Add(vector);
					StoreProcessedPath(path);
					return ObstaclePathingResult.Avoided;
				}
				if (CalcPathLength(s_ProcessedPath) < 1f)
				{
					s_ProcessedPath.Clear();
				}
				StoreProcessedPath(path);
				return ObstaclePathingResult.NoPath;
			}
			return ObstaclePathingResult.PathClear;
		}
		finally
		{
			s_Unit = null;
			s_Group = null;
			s_Points = null;
			s_ProcessedPath.Clear();
			s_IgnoredObstacles.Clear();
			s_LeftStack.Clear();
			s_RightStack.Clear();
		}
	}

	private static bool DoPathing(IntersectionResult intersection, Vector3 s, Vector3 t)
	{
		if (intersection.Unit1 == null)
		{
			return false;
		}
		s_Group = intersection.Unit1.GetObstaclesGroup();
		if (GetGroupSize(s_Group) <= 1)
		{
			return false;
		}
		List<Vector3> list = PathOneSide(intersection, s, t, 1);
		List<Vector3> list2 = PathOneSide(intersection, s, t, -1);
		if (list == null && list2 == null)
		{
			AddCutPoint(intersection, s, t);
			return false;
		}
		if (CalcPathLength(list) < CalcPathLength(list2))
		{
			AddPath(list);
		}
		else
		{
			AddPath(list2);
		}
		return true;
	}

	private static int GetGroupSize([CanBeNull] IReadOnlyList<UnitMovementAgent> group)
	{
		if (group == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < group.Count; i++)
		{
			if (!s_IgnoredObstacles.Contains(group[i]))
			{
				num++;
			}
		}
		return num;
	}

	private static float CalcPathLength(IList<Vector3> path)
	{
		if (path == null)
		{
			return float.MaxValue;
		}
		if (path.Count <= 0)
		{
			return float.MaxValue;
		}
		float num = 0f;
		for (int i = 1; i < path.Count; i++)
		{
			num += (path[i] - path[i - 1]).To2D().magnitude;
		}
		return num;
	}

	private static void AddCutPoint(IntersectionResult intersection, Vector3 s, Vector3 t)
	{
		if (!(intersection.Unit1 == null))
		{
			Vector3 position = intersection.Unit1.transform.position;
			Vector3 secondPoint = intersection.GetSecondPoint();
			Vector2 normalized = (position - secondPoint).To2D().normalized;
			Vector2 vector = new Vector2(normalized.y, 0f - normalized.x);
			if (Vector2.Dot((t - s).To2D(), vector) > 0f)
			{
				vector = -vector;
			}
			s_ProcessedPath.Add(intersection.Point + vector.To3D());
		}
	}

	private static void AddPath(IList<Vector3> path)
	{
		for (int i = 1; i < path.Count - 1; i++)
		{
			s_ProcessedPath.Add(path[i]);
		}
	}

	private static void StoreProcessedPath([NotNull] Path path)
	{
		path.vectorPath.Clear();
		foreach (Vector3 item in s_ProcessedPath)
		{
			path.vectorPath.Add(item);
		}
	}

	[CanBeNull]
	private static List<Vector3> PathOneSide(IntersectionResult intersection, Vector3 s, Vector3 t, int direction)
	{
		if (!intersection.HasIntersection || intersection.Unit1 == null)
		{
			PFLog.Default.Warning("intersection expected.");
			return null;
		}
		Vector3 secondPoint = intersection.GetSecondPoint();
		if (GeometryUtils.SignedAngle(b: (intersection.Unit1.transform.position - s).To2D(), a: (secondPoint - s).To2D()) * (float)direction > 0f)
		{
			if (intersection.Unit2 == null)
			{
				return null;
			}
			Utils.Swap(ref intersection.Unit1, ref intersection.Unit2);
		}
		Vector3 vector = intersection.GetSecondPoint();
		UnitMovementAgent unitMovementAgent = intersection.Unit1;
		ConvexHullStack convexHullStack = ((direction > 0) ? s_LeftStack : s_RightStack);
		convexHullStack.Clear();
		convexHullStack.Push(s);
		int num = 0;
		while (true)
		{
			if (++num > 1000)
			{
				PFLog.Default.Error("infinite loop");
				return null;
			}
			UnitMovementAgent unitMovementAgent2 = FindNextUnit(vector, unitMovementAgent, direction);
			if (unitMovementAgent2 == null)
			{
				return null;
			}
			if (IsSoftMode(s_Unit, unitMovementAgent2))
			{
				convexHullStack.Push((vector + unitMovementAgent2.transform.position) / 2f);
				convexHullStack.Push(t);
				return convexHullStack.Points;
			}
			Vector3 vector2 = CalcOffsetPoint(vector, unitMovementAgent, unitMovementAgent2, direction);
			Vector3 vector3 = Vector3.Lerp(unitMovementAgent.transform.position, vector2, 0.1f);
			if (UnitBlocksDirection(convexHullStack.Peek(), vector3, unitMovementAgent))
			{
				return null;
			}
			convexHullStack.Push(vector2);
			if (CanSeePoint(vector3, t, unitMovementAgent))
			{
				convexHullStack.Push(t);
				if (convexHullStack.Points.Count <= 2)
				{
					return null;
				}
				return convexHullStack.Points;
			}
			if (unitMovementAgent == intersection.Unit2 && unitMovementAgent2 == intersection.Unit1)
			{
				break;
			}
			vector = unitMovementAgent.transform.position;
			unitMovementAgent = unitMovementAgent2;
		}
		return null;
	}

	private static Vector3 CalcOffsetPoint(Vector3 prev, UnitMovementAgent curr, UnitMovementAgent next, int direction)
	{
		Vector2 v;
		if (Vector2.Distance((Vector2)next.transform.position, (Vector2)prev) < 0.01f)
		{
			v = (next.transform.position - curr.transform.position).To2D().normalized;
		}
		else
		{
			Vector2 normalized = (next.transform.position - prev).To2D().normalized;
			v = new Vector2(normalized.y, 0f - normalized.x) * direction;
		}
		Vector3 position = curr.transform.position;
		float num = s_Unit.Corpulence + curr.Corpulence;
		Vector3 vector = curr.transform.position - num * v.To3D();
		Vector3 vector2 = ObstacleAnalyzer.TraceAlongNavmesh(position, vector);
		if (vector2 != vector)
		{
			vector = Vector3.Lerp(position, vector2, 0.9f);
		}
		return vector;
	}

	private static bool CanSeePoint(Vector3 s, Vector3 t, [CanBeNull] UnitMovementAgent preferredCheck = null)
	{
		IntersectionResult result = default(IntersectionResult);
		if (preferredCheck != null)
		{
			IntersectObstacle(preferredCheck, s, t, ref result, findClosestPoint: false);
			if (result.HasIntersection)
			{
				return false;
			}
		}
		for (int i = 0; i < s_Group.Count; i++)
		{
			IntersectObstacle(s_Group[i], s, t, ref result, findClosestPoint: false);
			if (result.HasIntersection)
			{
				return false;
			}
		}
		return true;
	}

	private static bool UnitBlocksDirection(Vector3 s, Vector3 t, [NotNull] UnitMovementAgent unit)
	{
		IntersectionResult result = default(IntersectionResult);
		IntersectObstacle(unit, s, t, ref result, findClosestPoint: false);
		return result.HasIntersection;
	}

	[CanBeNull]
	private static UnitMovementAgent FindNextUnit(Vector3 prev, UnitMovementAgent curr, int direction)
	{
		Vector2 a = (prev - curr.transform.position).To2D();
		UnitMovementAgent unitMovementAgent = null;
		float num = 0f;
		foreach (UnitMovementAgent unitContact in curr.GetUnitContacts())
		{
			if (!s_IgnoredObstacles.Contains(unitContact))
			{
				Vector2 b = (unitContact.transform.position - curr.transform.position).To2D();
				float num2 = GeometryUtils.SignedAngle(a, b) * (float)direction;
				if (num2 <= 1f)
				{
					num2 += 360f;
				}
				if (unitMovementAgent == null || num2 < num)
				{
					unitMovementAgent = unitContact;
					num = num2;
				}
			}
		}
		return unitMovementAgent;
	}

	private static IntersectionResult IntersectAllObstacles(IList<UnitMovementAgent> units, Vector3 s, Vector3 t)
	{
		IntersectionResult result = default(IntersectionResult);
		for (int i = 0; i < units.Count; i++)
		{
			if (units[i].IsValid() && !units[i].AvoidanceDisabled)
			{
				IntersectObstacle(units[i], s, t, ref result, findClosestPoint: true);
			}
		}
		return result;
	}

	private static void IntersectObstacle([NotNull] UnitMovementAgent unit, Vector3 s, Vector3 t, ref IntersectionResult result, bool findClosestPoint)
	{
		if (unit.AvoidanceDisabled || s_IgnoredObstacles.Contains(unit) || IsSoftMode(s_Unit, unit) || !unit.GetUnitContacts().Any())
		{
			return;
		}
		float num = unit.Corpulence + 0.65f + 0.01f;
		if (VectorMath.SqrDistancePointSegment(s, t, unit.transform.position) > num * num)
		{
			return;
		}
		foreach (UnitMovementAgent unitContact in unit.GetUnitContacts())
		{
			if (s_IgnoredObstacles.Contains(unitContact))
			{
				continue;
			}
			if (findClosestPoint)
			{
				bool intersects;
				Vector3 vector = VectorMath.SegmentIntersectionPointXZ(s, t, unit.transform.position, unitContact.transform.position, out intersects);
				if (intersects)
				{
					result.Update(vector, (s - vector).sqrMagnitude, unit, unitContact);
				}
			}
			else if (VectorMath.SegmentsIntersectXZ(s, t, unit.transform.position, unitContact.transform.position))
			{
				result.HasIntersection = true;
				break;
			}
		}
	}

	private static bool IsSoftMode(UnitMovementAgent u1, UnitMovementAgent u2)
	{
		PartFaction partFaction = ((!(u1.Unit != null)) ? null : u1.Unit.EntityData?.GetFactionOptional());
		PartFaction partFaction2 = ((!(u2.Unit != null)) ? null : u2.Unit.EntityData?.GetFactionOptional());
		if (partFaction != null && partFaction2 != null)
		{
			return partFaction != partFaction2;
		}
		return false;
	}
}
