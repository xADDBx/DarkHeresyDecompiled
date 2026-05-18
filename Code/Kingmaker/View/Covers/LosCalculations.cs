using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.Covers;

public static class LosCalculations
{
	public enum CoverType
	{
		[FormerlySerializedAs("None")]
		Obstacle,
		[FormerlySerializedAs("Half")]
		Cover,
		[FormerlySerializedAs("Invisible")]
		LosBlocker
	}

	public readonly struct DistanceHeight
	{
		public readonly float Factor;

		public readonly float Height;

		public readonly GridNodeBase Node;

		public DistanceHeight(float factor, float height, GridNodeBase node)
		{
			Factor = factor;
			Height = height;
			Node = node;
		}

		public override string ToString()
		{
			return $"{Node.AsString()} H={Height} ({Factor * 100f}%)";
		}
	}

	private struct HeightAccumulatorVisitor : Linecast.ICanTransitionBetweenCells
	{
		private float m_Height;

		private Vector3 m_Origin;

		private Vector3 m_Direction;

		private float m_Distance;

		private readonly List<DistanceHeight> m_Heights;

		public HeightAccumulatorVisitor(Vector3 from, Vector3 to, Vector3 eyeShift, List<DistanceHeight> heights)
		{
			m_Height = from.y;
			m_Heights = heights;
			m_Origin = from + eyeShift;
			Vector3 vector = to + eyeShift - m_Origin;
			m_Distance = vector.magnitude;
			m_Direction = vector.normalized;
		}

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			float y = nodeFrom.Vector3Position().y;
			float y2 = nodeTo.Vector3Position().y;
			GridObstacleDescription? obstacleWithNode = nodeFrom.GetObstacleWithNode(nodeTo);
			if (obstacleWithNode.HasValue)
			{
				GridObstacleDescription valueOrDefault = obstacleWithNode.GetValueOrDefault();
				int top = valueOrDefault.Top;
				int bottom = valueOrDefault.Bottom;
				float num = (float)bottom * 0.001f;
				if ((m_Origin + m_Direction * m_Distance * distanceFactor).y < num)
				{
					return true;
				}
				float num2 = (float)top * 0.001f;
				if (Math.Abs(num2 - y) > 1f)
				{
					m_Height = num2;
				}
				m_Heights.Add(new DistanceHeight(distanceFactor, m_Height, nodeTo));
				if (Math.Abs(num2 - y2) > 1f)
				{
					m_Height = y2;
				}
				m_Heights.Add(new DistanceHeight(distanceFactor, m_Height, nodeTo));
			}
			return true;
		}
	}

	private struct FairLinecastTransition : Linecast.ICanTransitionBetweenCells
	{
		private Vector3 m_Origin;

		private Vector3 m_Direction;

		private float m_Distance;

		private Vector3? m_PrevRayElevation;

		public FairLinecastTransition(Vector3 from, Vector3 to, Vector3 eyeShift)
		{
			m_Origin = from + eyeShift;
			Vector3 vector = to + eyeShift - m_Origin;
			m_Distance = vector.magnitude;
			m_Direction = vector.normalized;
			m_PrevRayElevation = null;
		}

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			Vector3 value = m_Origin + m_Direction * m_Distance * distanceFactor;
			if (nodeFrom.IsFloor())
			{
				Vector3? prevRayElevation = m_PrevRayElevation;
				if (prevRayElevation.HasValue)
				{
					Vector3 valueOrDefault = prevRayElevation.GetValueOrDefault();
					float y = nodeFrom.Vector3Position().y;
					if ((valueOrDefault.y < y && value.y > y) || (valueOrDefault.y > y && value.y < y))
					{
						return false;
					}
				}
			}
			m_PrevRayElevation = value;
			GridObstacleDescription? obstacleWithNode = nodeFrom.GetObstacleWithNode(nodeTo);
			if (obstacleWithNode.HasValue)
			{
				GridObstacleDescription valueOrDefault2 = obstacleWithNode.GetValueOrDefault();
				int top = valueOrDefault2.Top;
				int bottom = valueOrDefault2.Bottom;
				float num = (int)(value.y * 1000f);
				if (!(num > (float)top))
				{
					return num < (float)bottom;
				}
				return true;
			}
			return true;
		}
	}

	public static readonly Vector3 EyeShift = Vector3.up * 1.5f;

	public const int CoverMin = 1100;

	public const int LosBlockerMin = 1500;

	private const float HeightThreshold = 1f;

	private const float EffectiveCoverAngleCosine = 0.9659258f;

	private static readonly ThreadLocal<List<DistanceHeight>> DstHeights = new ThreadLocal<List<DistanceHeight>>(() => new List<DistanceHeight>(128));

	public static LosDescription GetCellCoverStatus(GridNodeBase node, int direction)
	{
		GridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(direction, checkConnectivity: false);
		if (neighbourAlongDirection == null)
		{
			return new LosDescription(CoverType.LosBlocker);
		}
		if (HasForcedCover(neighbourAlongDirection, out var cover))
		{
			return new LosDescription(cover, new ObstacleInfo(neighbourAlongDirection));
		}
		int num = GraphHelper.OppositeDirections[direction];
		if (node.IsNodeCoveredByEntity(direction, out var coverEntity))
		{
			ObstacleInfo obstacle = new ObstacleInfo(neighbourAlongDirection, num, coverEntity);
			return new LosDescription(CoverType.Cover, obstacle);
		}
		if (node.GetNeighbourAlongDirection(direction) != null)
		{
			return new LosDescription(CoverType.Obstacle);
		}
		GridObstacleDescription? obstacleWithNode = node.GetObstacleWithNode(neighbourAlongDirection);
		if (obstacleWithNode.HasValue)
		{
			GridObstacleDescription valueOrDefault = obstacleWithNode.GetValueOrDefault();
			int num2 = valueOrDefault.Top - node.position.y;
			switch (valueOrDefault.Type)
			{
			case CoverType.LosBlocker:
				if (num2 > 1500)
				{
					return new LosDescription(CoverType.LosBlocker, neighbourAlongDirection, num);
				}
				break;
			case CoverType.Cover:
				if (num2 > 0)
				{
					return new LosDescription(CoverType.Cover, neighbourAlongDirection, num);
				}
				break;
			}
		}
		return new LosDescription(CoverType.Obstacle);
	}

	private static bool HasForcedCover(GridNodeBase node, out CoverType cover)
	{
		cover = CoverType.Obstacle;
		return Game.Instance?.Controllers.ForcedCoversController.TryGetCoverType(node, out cover) ?? false;
	}

	private static LosDescription GetEffectiveCover(GridNodeBase start, GridNodeBase end)
	{
		int2 @int = new int2(start.XCoordinateInGrid, start.ZCoordinateInGrid);
		int2 int2 = new int2(end.XCoordinateInGrid, end.ZCoordinateInGrid);
		(int x, int y) tangentNeighboursIdx = GetTangentNeighboursIdx(@int, int2);
		int item = tangentNeighboursIdx.x;
		int item2 = tangentNeighboursIdx.y;
		bool flag = end.Vector3Position().y - start.Vector3Position().y < 1f.Cells().Meters;
		if (item >= 0 && item2 >= 0)
		{
			int direction = ((item != 1) ? ((item2 == 0) ? 7 : 6) : ((item2 == 0) ? 4 : 5));
			LosDescription cellCoverStatus = GetCellCoverStatus(end, direction);
			int num = Math.Max(Math.Abs(int2.y - @int.y), Math.Abs(int2.x - @int.x));
			if ((CoverType)cellCoverStatus == CoverType.Cover)
			{
				cellCoverStatus = ((num < 3 && flag) ? cellCoverStatus.WithNewCoverType(CoverType.Obstacle) : GetEffectiveCoverWithRespectToAngle(@int, end, cellCoverStatus, int2));
				if (cellCoverStatus.OriginalCoverType == CoverType.Cover)
				{
					return cellCoverStatus;
				}
			}
		}
		GridNodeBase gridNodeBase = ((item >= 0) ? end.GetNeighbourAlongDirection(item, checkConnectivity: false) : null);
		GridNodeBase gridNodeBase2 = ((item2 >= 0) ? end.GetNeighbourAlongDirection(item2, checkConnectivity: false) : null);
		float num2 = ((gridNodeBase != null) ? (gridNodeBase.Vector3Position() - start.Vector3Position()).ToXZ().sqrMagnitude : float.MaxValue);
		float num3 = ((gridNodeBase2 != null) ? (gridNodeBase2.Vector3Position() - start.Vector3Position()).ToXZ().sqrMagnitude : float.MaxValue);
		LosDescription coverX = new LosDescription(CoverType.Obstacle);
		LosDescription coverY = new LosDescription(CoverType.Obstacle);
		bool flag2 = Math.Abs(num2 - num3) < 0.1f;
		if (item >= 0)
		{
			LosDescription losDescription = GetCellCoverStatus(end, item);
			int num4 = Math.Abs(int2.x - @int.x);
			if ((flag && num4 < 3 && (CoverType)losDescription == CoverType.Cover) || (num4 < 2 && flag2))
			{
				losDescription = losDescription.WithNewCoverType(CoverType.Obstacle);
			}
			coverX = losDescription;
		}
		if (item2 >= 0)
		{
			LosDescription losDescription2 = GetCellCoverStatus(end, item2);
			int num5 = Math.Abs(int2.y - @int.y);
			if ((flag && num5 < 3 && (CoverType)losDescription2 == CoverType.Cover) || (num5 < 2 && flag2))
			{
				losDescription2 = losDescription2.WithNewCoverType(CoverType.Obstacle);
			}
			coverY = losDescription2;
		}
		return SelectCover(coverX, num2, coverY, num3);
	}

	private static LosDescription SelectCover(LosDescription coverX, float distanceX, LosDescription coverY, float distanceY)
	{
		CoverType coverType = coverX.CoverType;
		CoverType coverType2 = coverY.CoverType;
		CoverType originalCoverType = coverX.OriginalCoverType;
		CoverType originalCoverType2 = coverY.OriginalCoverType;
		if (Math.Abs(distanceX - distanceY) > 0.1f)
		{
			if (distanceX < distanceY)
			{
				return coverX;
			}
			return coverY;
		}
		if (coverType > coverType2)
		{
			return coverX;
		}
		if (coverType == coverType2 && originalCoverType > originalCoverType2)
		{
			return coverX;
		}
		return coverY;
	}

	private static LosDescription GetEffectiveCoverWithRespectToAngle(int2 originPos, GridNodeBase end, LosDescription coverStatus, int2 endPos)
	{
		if (Vector2.Dot((coverStatus.ObstacleNode.Vector3Position() - end.Vector3Position()).To2D().normalized, new Vector2(originPos.x - endPos.x, originPos.y - endPos.y).normalized) <= 0.9659258f)
		{
			coverStatus = new LosDescription(CoverType.Obstacle);
		}
		return coverStatus;
	}

	public static Vector3 GetBestShootingPosition(MechanicEntity from, MechanicEntity to)
	{
		return GetBestShootingPosition(from.Position, from.SizeRect, to.Position, to.SizeRect);
	}

	public static Vector3 GetBestShootingPosition(Vector3 shooterPos, IntRect shooterSize, Vector3 targetPos, IntRect targetSize)
	{
		GridNodeBase nearestNodeXZUnwalkable = shooterPos.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = targetPos.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return shooterPos;
		}
		if (!(nearestNodeXZUnwalkable.Graph is GridGraph))
		{
			return shooterPos;
		}
		return GetBestShootingNode(nearestNodeXZUnwalkable, shooterSize, nearestNodeXZUnwalkable2, targetSize).Vector3Position();
	}

	public static GridNodeBase GetBestShootingNode(GridNodeBase origin, IntRect shooterSize, GridNodeBase end, IntRect targetSize, MechanicEntity currentEntityHit = null)
	{
		return origin;
	}

	public static bool HasLos(GridNodeBase origin, IntRect originSize, GridNodeBase end, IntRect endSize)
	{
		ObstacleInfo obstacle;
		return HasLos(origin, originSize, end, endSize, out obstacle);
	}

	public static bool HasLos(GridNodeBase origin, IntRect originSize, GridNodeBase end, IntRect endSize, out ObstacleInfo obstacle)
	{
		NodeList borderNodes = GridAreaHelper.GetBorderNodes(origin, originSize, origin.CoordinatesInGrid.SimplifiedDirection(end.CoordinatesInGrid));
		NodeList borderNodes2 = GridAreaHelper.GetBorderNodes(end, endSize);
		return HasLos(borderNodes, borderNodes2, out obstacle);
	}

	public static bool HasLos(MechanicEntity origin, MechanicEntity target)
	{
		ObstacleInfo obstacle;
		return HasLos(origin.GetOccupiedNodes(), target.GetOccupiedNodes(), out obstacle);
	}

	private static bool HasLos(NodeList from, NodeList to, out ObstacleInfo obstacle)
	{
		LosDescription los;
		return HasLos(from, to, out obstacle, out los);
	}

	private static bool HasLos(NodeList from, NodeList to, out ObstacleInfo obstacle, out LosDescription los)
	{
		los = default(LosDescription);
		bool flag = HasLosInternal(from, to, isMelee: false, out obstacle);
		if (!flag)
		{
			GridNodeBase start = from.First();
			GridNodeBase end = to.First();
			los = GetEffectiveCover(start, end);
			return false;
		}
		CoverType coverType = CoverType.LosBlocker;
		bool flag2 = false;
		foreach (GridNodeBase item in from)
		{
			foreach (GridNodeBase item2 in to)
			{
				LosDescription effectiveCover = GetEffectiveCover(item, item2);
				if (effectiveCover.CoverType <= coverType)
				{
					los = effectiveCover;
					coverType = effectiveCover.CoverType;
					if (coverType == CoverType.Obstacle)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (flag2)
			{
				break;
			}
		}
		if (flag)
		{
			obstacle = los.Obstacle;
		}
		if (flag)
		{
			return los.CoverType < CoverType.LosBlocker;
		}
		return false;
	}

	private static bool HasLosInternal(NodeList from, NodeList to, bool isMelee, out ObstacleInfo obstacle)
	{
		obstacle = default(ObstacleInfo);
		foreach (GridNodeBase item in from)
		{
			foreach (GridNodeBase item2 in to)
			{
				if (GetDirectLoSInternal(item, item2, isMelee, out var obstacle2))
				{
					return true;
				}
				if (obstacle.Entity == null)
				{
					obstacle = obstacle2;
				}
			}
		}
		return false;
	}

	private static bool HasAdjacentDiagonalMeleeLos([NotNull] GridNodeBase from, [NotNull] GridNodeBase to, out ObstacleInfo obstacle)
	{
		obstacle = default(ObstacleInfo);
		if (!from.GetDirection(to).IsDiagonal())
		{
			return false;
		}
		int num = to.XCoordinateInGrid - from.XCoordinateInGrid;
		int num2 = to.ZCoordinateInGrid - from.ZCoordinateInGrid;
		if (Math.Abs(num) != 1 || Math.Abs(num2) != 1)
		{
			return false;
		}
		GridNodeDirection direction = ((num > 0) ? GridNodeDirection.E : GridNodeDirection.W);
		GridNodeDirection direction2 = ((num2 > 0) ? GridNodeDirection.N : GridNodeDirection.S);
		GridObstacleCache obstacles = AdditionalGraphDataManager.Instance.GetGraphData(from.GraphIndex).Obstacles;
		GridObstacleCache.Entry obstacle2 = obstacles.GetObstacle(from, direction);
		GridObstacleCache.Entry obstacle3 = obstacles.GetObstacle(from, direction2);
		bool num3 = obstacle2.Exists && obstacle2.Type >= CoverType.LosBlocker;
		bool flag = obstacle3.Exists && obstacle3.Type >= CoverType.LosBlocker;
		if (num3 && flag)
		{
			return false;
		}
		return true;
	}

	private static bool GetDirectLoSInternal([NotNull] GridNodeBase from, [NotNull] GridNodeBase to, bool isMelee, out ObstacleInfo obstacle)
	{
		GridGraph obj = (GridGraph)from.Graph;
		GridGraph gridGraph = (GridGraph)to.Graph;
		if (obj != gridGraph)
		{
			obstacle = default(ObstacleInfo);
			return false;
		}
		if (isMelee && Math.Abs(from.Vector3Position().y - to.Vector3Position().y) > 1.Cells().Meters)
		{
			obstacle = default(ObstacleInfo);
			return false;
		}
		if (isMelee && HasAdjacentDiagonalMeleeLos(from, to, out obstacle))
		{
			return true;
		}
		return GetOneLineDirectLoSInternal(from, to, out obstacle);
	}

	private static bool GetOneLineDirectLoSInternal(GridNodeBase from, GridNodeBase to, out ObstacleInfo obstacle)
	{
		return GetOneLineDirectLoSInternalWithNodeHeights(from, to, out obstacle);
	}

	public static bool GetOneLineDirectLosWithNodeHeights(GridNodeBase from, GridNodeBase to, [CanBeNull] List<DistanceHeight> nodeHeights)
	{
		ObstacleInfo obstacle;
		return GetOneLineDirectLoSInternalWithNodeHeights(from, to, out obstacle, nodeHeights);
	}

	private static bool GetOneLineDirectLoSInternalWithNodeHeights(GridNodeBase from, GridNodeBase to, out ObstacleInfo obstacle, [CanBeNull] List<DistanceHeight> _ = null)
	{
		if (from == to)
		{
			obstacle = default(ObstacleInfo);
			return true;
		}
		GridGraph graph = (GridGraph)from.Graph;
		Vector3 pointClosestToTargetOnNodeEdge = GetPointClosestToTargetOnNodeEdge(from, to);
		Vector3 pointClosestToTargetOnNodeEdge2 = GetPointClosestToTargetOnNodeEdge(to, from);
		FairLinecastTransition condition = new FairLinecastTransition(pointClosestToTargetOnNodeEdge, pointClosestToTargetOnNodeEdge2, EyeShift);
		if (Linecast.LinecastGrid(graph, pointClosestToTargetOnNodeEdge, pointClosestToTargetOnNodeEdge2, from, out var hit, null, ref condition, 0.0001f))
		{
			obstacle = new ObstacleInfo(hit.node, hit.nextNode);
			return false;
		}
		obstacle = default(ObstacleInfo);
		return true;
	}

	private static Vector3 GetPointClosestToTargetOnNodeEdge(GridNodeBase node, GridNodeBase targetNode)
	{
		Vector3 vector = node.Vector3Position().ToXZ();
		Vector3 normalized = (targetNode.Vector3Position().ToXZ() - vector).normalized;
		float gridCellSize = GraphParamsMechanicsCache.GridCellSize;
		Bounds bounds = new Bounds(vector, new Vector3(gridCellSize, 0f, gridCellSize));
		Ray ray = new Ray(vector, normalized);
		if (bounds.IntersectRay(ray, out float distance))
		{
			return node.Vector3Position() + normalized * Math.Abs(distance) * 0.99f;
		}
		return node.Vector3Position();
	}

	public static bool GetDirectLos(Vector3 origin, Vector3 end)
	{
		GridNodeBase nearestNodeXZUnwalkable = origin.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = end.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return false;
		}
		ObstacleInfo obstacle;
		return GetDirectLoSInternal(nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2, isMelee: false, out obstacle);
	}

	public static (GridNodeBase left, GridNodeBase right) GetOrthoNeighbours(GridNodeBase origin, Vector3 dir)
	{
		(int left, int right) orthoNeighboursIdx = GetOrthoNeighboursIdx(dir);
		int item = orthoNeighboursIdx.left;
		int item2 = orthoNeighboursIdx.right;
		GridNodeBase neighbourAlongDirection = origin.GetNeighbourAlongDirection(item);
		GridNodeBase neighbourAlongDirection2 = origin.GetNeighbourAlongDirection(item2);
		return (left: neighbourAlongDirection, right: neighbourAlongDirection2);
	}

	public static CoverType GetWarhammerLos(MechanicEntity from, Vector3 fromPosition, MechanicEntity to)
	{
		return GetWarhammerLos(fromPosition, from.SizeRect, to.Position, to.SizeRect);
	}

	private static (int left, int right) GetOrthoNeighboursIdx(Vector3 origin, Vector3 end)
	{
		return GetOrthoNeighboursIdx(end - origin);
	}

	private static (int left, int right) GetOrthoNeighboursIdx(Vector3 dir)
	{
		if (!(Mathf.Abs(dir.x) > Mathf.Abs(dir.z)))
		{
			return (left: 1, right: 3);
		}
		return (left: 2, right: 0);
	}

	private static int2 GetOrthoAxis(Int3 origin, Int3 end)
	{
		Int3 @int = end - origin;
		if (Mathf.Abs(@int.x) <= Mathf.Abs(@int.z))
		{
			return new int2(1, 0);
		}
		return new int2(0, 1);
	}

	private static (int x, int y) GetTangentNeighboursIdx(int2 origin, int2 end)
	{
		int2 @int = origin - end;
		int item = ((@int.x > 0) ? 1 : ((@int.x >= 0) ? (-1) : 3));
		int item2 = ((@int.y > 0) ? 2 : ((@int.y >= 0) ? (-1) : 0));
		return (x: item, y: item2);
	}

	public static CoverType GetCoverType(Vector3 position)
	{
		return GetCoverType((GridNode)(GraphNode)ObstacleAnalyzer.GetNearestNode(position));
	}

	public static CoverType GetCoverType(GridNode node)
	{
		CoverType result = CoverType.Obstacle;
		for (int i = 0; i < 4; i++)
		{
			LosDescription cellCoverStatus = GetCellCoverStatus(node, i);
			if ((CoverType)cellCoverStatus == CoverType.Cover)
			{
				return cellCoverStatus;
			}
			if ((CoverType)cellCoverStatus == CoverType.LosBlocker)
			{
				result = cellCoverStatus;
			}
		}
		return result;
	}

	public static LosDescription GetWarhammerLos(GridNodeBase origin, GridNodeBase end)
	{
		return GetWarhammerLos(origin, default(IntRect), end, default(IntRect));
	}

	public static LosDescription GetWarhammerLos(GridNodeBase origin, IntRect originSize, GridNodeBase end, IntRect endSize)
	{
		NodeList borderNodes = GridAreaHelper.GetBorderNodes(origin, originSize, origin.CoordinatesInGrid.SimplifiedDirection(end.CoordinatesInGrid));
		NodeList borderNodes2 = GridAreaHelper.GetBorderNodes(end, endSize);
		return GetWarhammerLos(borderNodes, borderNodes2);
	}

	public static LosDescription GetWarhammerLos(NodeList from, NodeList to)
	{
		using (ProfileScope.New("LosCalculations.GetWarhammerLos"))
		{
			if (!HasLos(from, to, out var obstacle, out var los))
			{
				return new LosDescription(CoverType.LosBlocker, obstacle);
			}
			if (to.NonSingle())
			{
				return new LosDescription(CoverType.Obstacle);
			}
			return los;
		}
	}

	public static LosDescription GetWarhammerLos(MechanicEntity entity, GridNodeBase endNode, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(endNode, endSize);
		return GetWarhammerLos(entity.GetOccupiedNodes(), nodes);
	}

	public static LosDescription GetWarhammerLos(MechanicEntity from, MechanicEntity to)
	{
		return GetWarhammerLos(from.GetOccupiedNodes(), to.GetOccupiedNodes());
	}

	public static LosDescription GetWarhammerLos(Vector3 origin, IntRect originSize, Vector3 end, IntRect endSize)
	{
		GridNodeBase nearestNodeXZUnwalkable = origin.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = end.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return new LosDescription(CoverType.LosBlocker);
		}
		NodeList nodes = GridAreaHelper.GetNodes(origin, originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end, endSize);
		return GetWarhammerLos(nodes, nodes2);
	}

	public static LosDescription GetWarhammerLos(MechanicEntity entity, Vector3 target, IntRect targetSize)
	{
		NodeList occupiedNodes = entity.GetOccupiedNodes();
		NodeList nodes = GridAreaHelper.GetNodes(target.GetNearestNodeXZUnwalkable(), targetSize);
		return GetWarhammerLos(occupiedNodes, nodes);
	}

	public static LosDescription GetWarhammerLos(Vector3 origin, IntRect fromSize, MechanicEntity target)
	{
		return GetWarhammerLos(GridAreaHelper.GetNodes(origin.GetNearestNodeXZUnwalkable(), fromSize), target.GetOccupiedNodes());
	}

	public static bool HasDirectLos([NotNull] this GridNodeBase from, [NotNull] GridNodeBase to)
	{
		ObstacleInfo obstacle;
		return GetDirectLoSInternal(from, to, isMelee: false, out obstacle);
	}

	public static bool HasMeleeLos(this MechanicEntity from, MechanicEntity to)
	{
		ObstacleInfo obstacle;
		return HasLosInternal(from.GetOccupiedNodes(), to.GetOccupiedNodes(), isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos(Vector3 origin, IntRect originSize, Vector3 end, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(origin.GetNearestNodeXZUnwalkable(), originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end.GetNearestNodeXZUnwalkable(), endSize);
		ObstacleInfo obstacle;
		return HasLosInternal(nodes, nodes2, isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos([NotNull] this GridNodeBase from, [NotNull] GridNodeBase to)
	{
		ObstacleInfo obstacle;
		return GetDirectLoSInternal(from, to, isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos(GridNodeBase origin, IntRect originSize, GridNodeBase end, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(origin, originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end, endSize);
		ObstacleInfo obstacle;
		return HasLosInternal(nodes, nodes2, isMelee: true, out obstacle);
	}
}
