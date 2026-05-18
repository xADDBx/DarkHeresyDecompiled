using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility.Locator;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class PathfindingService : IService
{
	public class Options
	{
		public IPathModifier[] Modifiers;
	}

	public struct PostProcessData
	{
		public float ApproachRadiusMeters;

		public PostProcessData(float approachRadiusMeters)
		{
			ApproachRadiusMeters = approachRadiusMeters;
		}
	}

	private struct WarhammerPathChargeCacheEntry
	{
		public EntityRef Entity;

		public WarhammerPathCharge Path;

		public Vector3 Origin;

		public Vector3 Destination;

		public bool IgnoreBlockers;

		public EntityRef TargetEntity;
	}

	private readonly List<(Path path, Action<Path> callback, int delayToStep)> m_DelayedPaths = new List<(Path, Action<Path>, int)>();

	private static ServiceProxy<PathfindingService> s_InstanceProxy;

	private TraversalProviderWithBusyLinkPenalties m_TraversalProviderWithBusyLinkPenalties = new TraversalProviderWithBusyLinkPenalties();

	private Queue<WarhammerPathChargeCacheEntry> m_ChargePathCache = new Queue<WarhammerPathChargeCacheEntry>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static PathfindingService Instance
	{
		get
		{
			if (s_InstanceProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new PathfindingService());
				s_InstanceProxy = Services.GetProxy<PathfindingService>();
			}
			return s_InstanceProxy.Instance;
		}
	}

	public TraversalProviderWithBusyLinkPenalties TraversalProviderWithBusyLinkPenalties => m_TraversalProviderWithBusyLinkPenalties;

	public void ForceCompleteAll()
	{
		PFLog.Pathfinding.Log($"Force complete all paths... x{m_DelayedPaths.Count}");
		int num = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		while (0 < m_DelayedPaths.Count)
		{
			List<(Path path, Action<Path> callback, int delayToStep)> delayedPaths = m_DelayedPaths;
			int num2 = delayedPaths[delayedPaths.Count - 1].delayToStep + 1;
			for (int i = num; i < num2; i++)
			{
				TickInternal(i);
			}
			num = num2;
		}
	}

	public void Tick()
	{
		int currentSystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		TickInternal(currentSystemStepIndex);
	}

	private void TickInternal(int currentSytemStep)
	{
		using (ProfileScope.New("PathindingService.Tick"))
		{
			int num = 0;
			for (int i = 0; i < m_DelayedPaths.Count; i++)
			{
				(Path, Action<Path>, int) tuple = m_DelayedPaths[i];
				if (tuple.Item3 > currentSytemStep)
				{
					break;
				}
				if (!tuple.Item1.IsDoneAndPostProcessed())
				{
					using (ProfileScope.New("BlockUntilCalculated"))
					{
						AstarPath.BlockUntilCalculated(tuple.Item1);
					}
				}
				try
				{
					using (ProfileScope.New("Invoke callback"))
					{
						tuple.Item2?.Invoke(tuple.Item1);
					}
				}
				catch (Exception exception)
				{
					PFLog.Pathfinding.ExceptionWithReport(exception, "Exception occured in delayed path callback");
				}
				if (tuple.Item2 != null)
				{
					tuple.Item1.Release(this);
				}
				num++;
			}
			m_DelayedPaths.RemoveRange(0, num);
		}
	}

	private static void PreProcess(Path path, [CanBeNull] Options options)
	{
		if (options?.Modifiers == null)
		{
			return;
		}
		using (ProfileScope.New("PathfindingService.PreProcess"))
		{
			IPathModifier[] modifiers = options.Modifiers;
			for (int i = 0; i < modifiers.Length; i++)
			{
				modifiers[i].PreProcess(path);
			}
		}
	}

	private static Vector3[] FindLineSphereIntersections(Vector3 linePoint0, Vector3 linePoint1, Vector3 circleCenter, float circleRadius)
	{
		float x = circleCenter.x;
		float y = circleCenter.y;
		float z = circleCenter.z;
		float x2 = linePoint0.x;
		float y2 = linePoint0.y;
		float z2 = linePoint0.z;
		float num = linePoint1.x - x2;
		float num2 = linePoint1.y - y2;
		float num3 = linePoint1.z - z2;
		float num4 = num * num + num2 * num2 + num3 * num3;
		float num5 = 2f * (x2 * num + y2 * num2 + z2 * num3 - num * x - num2 * y - num3 * z);
		float num6 = x2 * x2 - 2f * x2 * x + x * x + y2 * y2 - 2f * y2 * y + y * y + z2 * z2 - 2f * z2 * z + z * z - circleRadius * circleRadius;
		float num7 = num5 * num5 - 4f * num4 * num6;
		float num8 = (0f - num5 - Mathf.Sqrt(num7)) / (2f * num4);
		Vector3 vector = new Vector3(linePoint0.x * (1f - num8) + num8 * linePoint1.x, linePoint0.y * (1f - num8) + num8 * linePoint1.y, linePoint0.z * (1f - num8) + num8 * linePoint1.z);
		float num9 = (0f - num5 + Mathf.Sqrt(num7)) / (2f * num4);
		Vector3 vector2 = new Vector3(linePoint0.x * (1f - num9) + num9 * linePoint1.x, linePoint0.y * (1f - num9) + num9 * linePoint1.y, linePoint0.z * (1f - num9) + num9 * linePoint1.z);
		if (num7 < 0f)
		{
			return Array.Empty<Vector3>();
		}
		if ((num8 > 1f || num8 < 0f) && (num9 > 1f || num9 < 0f))
		{
			return Array.Empty<Vector3>();
		}
		if (!(num8 > 1f) && !(num8 < 0f) && (num9 > 1f || num9 < 0f))
		{
			return new Vector3[1] { vector };
		}
		if ((num8 > 1f || num8 < 0f) && !(num9 > 1f) && !(num9 < 0f))
		{
			return new Vector3[1] { vector2 };
		}
		if (num7 != 0f)
		{
			return new Vector3[2] { vector, vector2 };
		}
		return new Vector3[1] { vector };
	}

	private static Vector3 GetNewEndPoint(WarhammerXPath xPath, float approachRadiusMeters)
	{
		Vector3[] array = Array.Empty<Vector3>();
		if (xPath.vectorPath.Count > 2)
		{
			List<Vector3> vectorPath = xPath.vectorPath;
			Vector3 vector = vectorPath[vectorPath.Count - 2];
			List<Vector3> vectorPath2 = xPath.vectorPath;
			if ((vector - vectorPath2[vectorPath2.Count - 1]).sqrMagnitude < float.Epsilon)
			{
				List<Vector3> vectorPath3 = xPath.vectorPath;
				return vectorPath3[vectorPath3.Count - 1];
			}
			List<GraphNode> path = xPath.path;
			if (path[path.Count - 2] is LinkNode)
			{
				List<Vector3> vectorPath4 = xPath.vectorPath;
				return vectorPath4[vectorPath4.Count - 1];
			}
			List<Vector3> vectorPath5 = xPath.vectorPath;
			Vector3 linePoint = vectorPath5[vectorPath5.Count - 2];
			List<Vector3> vectorPath6 = xPath.vectorPath;
			Vector3[] array2 = FindLineSphereIntersections(linePoint, vectorPath6[vectorPath6.Count - 1], xPath.originalEndPoint, approachRadiusMeters);
			if (array2.Length != 0)
			{
				array = array2;
			}
		}
		else
		{
			Vector3 originalStartPoint = xPath.originalStartPoint;
			List<Vector3> vectorPath7 = xPath.vectorPath;
			if ((originalStartPoint - vectorPath7[vectorPath7.Count - 1]).sqrMagnitude < float.Epsilon)
			{
				List<Vector3> vectorPath8 = xPath.vectorPath;
				return vectorPath8[vectorPath8.Count - 1];
			}
			Vector3 originalStartPoint2 = xPath.originalStartPoint;
			List<Vector3> vectorPath9 = xPath.vectorPath;
			array = FindLineSphereIntersections(originalStartPoint2, vectorPath9[vectorPath9.Count - 1], xPath.originalEndPoint, approachRadiusMeters);
		}
		if (array.Length == 1)
		{
			return array[0];
		}
		if (array.Length == 2)
		{
			List<Vector3> vectorPath10 = xPath.vectorPath;
			float magnitude = (vectorPath10[vectorPath10.Count - 2] - array[0]).magnitude;
			List<Vector3> vectorPath11 = xPath.vectorPath;
			if (!(magnitude <= (vectorPath11[vectorPath11.Count - 2] - array[1]).magnitude))
			{
				return array[1];
			}
			return array[0];
		}
		List<Vector3> vectorPath12 = xPath.vectorPath;
		return vectorPath12[vectorPath12.Count - 1];
	}

	private static void PostProcess(Path path, PostProcessData? data, [CanBeNull] Options options)
	{
		if (options?.Modifiers != null)
		{
			using (ProfileScope.New("PathfindingService.PostProcess"))
			{
				IPathModifier[] modifiers = options.Modifiers;
				for (int i = 0; i < modifiers.Length; i++)
				{
					modifiers[i].Apply(path);
				}
			}
		}
		if (data.HasValue && !Game.Instance.Controllers.TurnController.TurnBasedModeActive && path is WarhammerXPath warhammerXPath)
		{
			Vector3 value = (warhammerXPath.endPoint = GetNewEndPoint(warhammerXPath, data.Value.ApproachRadiusMeters));
			List<Vector3> vectorPath = warhammerXPath.vectorPath;
			vectorPath[vectorPath.Count - 1] = value;
		}
	}

	public void FindPath<TPath>(TPath path, PostProcessData? data, [CanBeNull] Options options, Action<ForcedPath> callback) where TPath : Path
	{
		if (callback != null)
		{
			path.Claim(this);
		}
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		path.enabledTags = 1;
		path.callback = delegate(Path p)
		{
			OnForcedPathCompleteInternal<TPath>(p as TPath, data, options, callback);
		};
		PreProcess(path, options);
		AstarPath.StartPath(path);
	}

	public void FindPathWithType<TPath>(TPath path, PostProcessData? data, [CanBeNull] Options options, Action<TPath> callback) where TPath : Path
	{
		if (callback != null)
		{
			path.Claim(this);
		}
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		path.enabledTags = 1;
		path.callback = delegate(Path p)
		{
			OnPathCompleteInternal(p, data, options, callback);
		};
		PreProcess(path, options);
		AstarPath.StartPath(path);
	}

	private void OnPathCompleteInternal<TPath>(Path path, PostProcessData? data, [CanBeNull] Options options, Action<TPath> callback) where TPath : Path
	{
		if (!path.error)
		{
			PostProcess(path, data, options);
		}
		if (callback != null)
		{
			try
			{
				callback(path as TPath);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
			path.Release(this);
		}
	}

	private void OnForcedPathCompleteInternal<TPath>(Path path, PostProcessData? data, [CanBeNull] Options options, Action<ForcedPath> callback) where TPath : Path
	{
		if (!path.error)
		{
			PostProcess(path, data, options);
		}
		if (callback != null)
		{
			ForcedPath obj = (path.error ? ForcedPath.ErrorPath : ForcedPath.Construct(path));
			try
			{
				callback(obj);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
			path.Release(this);
		}
	}

	private static void FindPath_Blocking(Path path, [CanBeNull] Options options)
	{
		path.enabledTags = 1;
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		PreProcess(path, options);
		AstarPath.StartPath(path);
		AstarPath.BlockUntilCalculated(path);
		PostProcess(path, null, options);
	}

	private ABPath ConstructPath([NotNull] UnitMovementAgent agent, Vector3 destination, float approachRadiusMeters, ICustomDistanceCheck customCondition = null)
	{
		Vector3 start = agent.Position;
		if (agent.IsTraverseInProgress)
		{
			start = agent.NodeLinkTraverser.DestinationNode.Vector3Position();
		}
		else if (agent.IsReallyMoving)
		{
			start = agent.SimulateSimplifiedMovement(2);
		}
		if (approachRadiusMeters > 0.35f)
		{
			WarhammerXPath warhammerXPath = WarhammerXPath.Construct(start, destination);
			warhammerXPath.nnConstraint.constrainArea = true;
			warhammerXPath.endingCondition = new LosPathCondition(warhammerXPath, approachRadiusMeters - 0.2f, checkLos: false, customCondition);
			warhammerXPath.LinkTraversalProvider = agent.NodeLinkTraverser;
			warhammerXPath.traversalProvider = m_TraversalProviderWithBusyLinkPenalties;
			return warhammerXPath;
		}
		WarhammerABPath warhammerABPath = WarhammerABPath.Construct(start, destination);
		warhammerABPath.nnConstraint.constrainArea = true;
		warhammerABPath.LinkTraversalProvider = agent.NodeLinkTraverser;
		warhammerABPath.traversalProvider = m_TraversalProviderWithBusyLinkPenalties;
		return warhammerABPath;
	}

	public ABPath FindPathRT([NotNull] UnitMovementAgent agent, Vector3 destination, float approachRadiusMeters, [NotNull] Action<ForcedPath> callback, ICustomDistanceCheck customCondition = null)
	{
		return FindPathRT_Delayed(agent, destination, approachRadiusMeters, 1, callback, customCondition);
	}

	public ABPath FindPathRT_Delayed([NotNull] UnitMovementAgent agent, Vector3 destination, float approachRadiusMeters, int delaySteps, [NotNull] Action<ForcedPath> callback, ICustomDistanceCheck customCondition = null)
	{
		ABPath aBPath = ConstructPath(agent, destination, approachRadiusMeters, customCondition);
		aBPath.Claim(this);
		FindPathWithType(aBPath, new PostProcessData(approachRadiusMeters), agent.RealTimeOptions, null);
		AddDelayedPath((path: aBPath, callback: delegate(Path p)
		{
			ForcedPath obj = ForcedPath.Construct(p);
			try
			{
				callback(obj);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
		}, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return aBPath;
	}

	public Task<ForcedPath> FindPathRT_Async([NotNull] UnitMovementAgent agent, Vector3 destination, float approachRadiusMeters, int delaySteps)
	{
		TaskCompletionSource<ForcedPath> tcs = new TaskCompletionSource<ForcedPath>();
		FindPathRT_Delayed(agent, destination, approachRadiusMeters, delaySteps, delegate(ForcedPath path)
		{
			tcs.SetResult(path);
		});
		return tcs.Task;
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed([NotNull] UnitMovementAgent agent, Vector3 destination, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost: false, oneWayLinksAreForbidden: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed([NotNull] UnitMovementAgent agent, MechanicEntity targetEntity, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), null, targetEntity, ignoreThreateningAreaCost: false, oneWayLinksAreForbidden: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed([NotNull] UnitMovementAgent agent, TargetWrapper target, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), target.Point, target.Entity, ignoreThreateningAreaCost: false, oneWayLinksAreForbidden: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public ForcedPath FindPathRT_Blocking([NotNull] UnitMovementAgent agent, Vector3 destination, float approachRadiusMeters, ICustomDistanceCheck customCondition = null)
	{
		ABPath aBPath = ConstructPath(agent, destination, approachRadiusMeters, customCondition);
		aBPath.Claim(this);
		FindPath_Blocking(aBPath, agent.RealTimeOptions);
		if (aBPath.error)
		{
			aBPath.Release(this);
			return ForcedPath.ErrorPath;
		}
		ForcedPath result = ForcedPath.Construct(aBPath);
		aBPath.Release(this);
		return result;
	}

	public ForcedPath FindPathBetweenPointsRT_Blocking(Vector3 start, Vector3 destination)
	{
		WarhammerABPath warhammerABPath = WarhammerABPath.Construct(start, destination);
		warhammerABPath.nnConstraint.constrainArea = true;
		warhammerABPath.Claim(this);
		FindPath_Blocking(warhammerABPath, null);
		if (warhammerABPath.error)
		{
			warhammerABPath.Release(this);
			return ForcedPath.ErrorPath;
		}
		ForcedPath result = ForcedPath.Construct(warhammerABPath);
		warhammerABPath.Release(this);
		return result;
	}

	private WarhammerPathPlayer ConstructWhPlayerPath([NotNull] UnitMovementAgent agent, Vector3 start, float maxLength, Vector3? destination, MechanicEntity targetEntity, bool ignoreThreateningAreaCost, bool oneWayLinksAreForbidden, bool ignoreBlockers = false)
	{
		ignoreBlockers |= (bool)agent.Unit?.Data.Features.CanMoveThroughEnemies;
		GridNode targetNode = ((targetEntity != null) ? null : (destination.HasValue ? ObstacleAnalyzer.GetNearestNodeXZUnwalkable(destination.Value) : null));
		WarhammerPathPlayerMetricCostProvider traversalCostProvider = new WarhammerPathPlayerMetricCostProvider(agent.Unit.Data, ignoreThreateningAreaCost);
		WarhammerPathPlayerMetric initialLength = new WarhammerPathPlayerMetric(agent.Unit.Data.GetCombatStateOptional()?.LastDiagonalCount ?? 0, agent.Unit.Data.GetCombatStateOptional()?.LastDiagonalCount ?? 0, 0, 0f, 0, isOneWayPath: false);
		WarhammerPathPlayer warhammerPathPlayer = WarhammerPathPlayer.Construct(agent.Unit.Data, start, maxLength, targetNode, targetEntity, ignoreBlockers ? BlockMode.Ignore : agent.Unit.BlockMode, agent.Unit?.Data.Features.PassThroughSmallUnits, initialLength, traversalCostProvider, oneWayLinksAreForbidden);
		warhammerPathPlayer.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		warhammerPathPlayer.traversalProvider = agent.TraversalProvider;
		warhammerPathPlayer.LinkTraversalProvider = agent.NodeLinkTraverser;
		return warhammerPathPlayer;
	}

	private WarhammerPathCharge ConstructWhChargePath([NotNull] UnitMovementAgent agent, Vector3 start, Vector3 destination, float maxLength, bool ignoreBlockers, [CanBeNull] MechanicEntity targetEntity)
	{
		BaseUnitEntity unit = (agent.Unit.Data as BaseUnitEntity) ?? throw new Exception("Only an entity based on BaseUnitEntity can use ConstructWhChargePath.");
		GridNodeBase nearestNodeXZUnwalkable = start.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = destination.GetNearestNodeXZUnwalkable();
		WarhammerPathChargeMetricCostProvider traversalCostProvider = new WarhammerPathChargeMetricCostProvider(unit, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
		WarhammerPathCharge obj = WarhammerPathCharge.Construct(initialLength: new WarhammerPathChargeMetric(0f, 0), unit: agent.Unit.Data, start: start, targetNode: nearestNodeXZUnwalkable2, targetEntity: targetEntity, maxLength: maxLength, blockMode: ignoreBlockers ? BlockMode.Ignore : agent.Unit.BlockMode, passThroughSmallUnits: agent.Unit.Data.Features.PassThroughSmallUnits, traversalCostProvider: traversalCostProvider);
		obj.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		obj.traversalProvider = agent.TraversalProvider;
		obj.persistentPath = true;
		return obj;
	}

	private WarhammerPathAi ConstructWarhammerPathAi([NotNull] UnitMovementAgent agent, Vector3 start, float maxLength, WarhammerPathAiMetric initialLength, ITraversalCostProvider<WarhammerPathAiMetric> traversalCostProvider)
	{
		WarhammerPathAi warhammerPathAi = WarhammerPathAi.Construct(agent.Unit.Data, start, maxLength, null, null, agent.Unit.BlockMode, agent.Unit.Data.Features.PassThroughSmallUnits, initialLength, traversalCostProvider);
		warhammerPathAi.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		warhammerPathAi.traversalProvider = agent.TraversalProvider;
		warhammerPathAi.LinkTraversalProvider = agent.NodeLinkTraverser;
		return warhammerPathAi;
	}

	public WarhammerPathPlayer FindPathTB_Blocking([NotNull] UnitMovementAgent agent, Vector3 destination, bool limitRangeByActionPoints = true, bool ignoreThreateningAreaCost = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost, oneWayLinksAreForbidden: false);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathPlayer FindPathTB_Blocking([NotNull] UnitMovementAgent agent, MechanicEntity targetEntity, bool limitRangeByActionPoints = true)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), null, targetEntity, ignoreThreateningAreaCost: false, oneWayLinksAreForbidden: false);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgent agent, Vector3 origin, Vector3 destination, bool limitRangeByActionPoints = true, bool ignoreThreateningAreaCost = false, bool ignoreBlockers = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, origin, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.MovementPoints ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost, oneWayLinksAreForbidden: false, ignoreBlockers);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathCharge FindPathChargeTB_Blocking(UnitMovementAgent agent, Vector3 origin, Vector3 destination, float maxLength, bool ignoreBlockers, [CanBeNull] MechanicEntity targetEntity)
	{
		WarhammerPathCharge warhammerPathCharge = ConstructWhChargePath(agent, origin, destination, maxLength, ignoreBlockers, targetEntity);
		foreach (WarhammerPathChargeCacheEntry item in m_ChargePathCache)
		{
			if (item.Entity.Id == agent.Unit?.UniqueId && item.Origin == origin && item.Destination == destination && item.IgnoreBlockers == ignoreBlockers)
			{
				EntityRef targetEntity2 = item.TargetEntity;
				if ((targetEntity2.IsEmpty && targetEntity == null) || item.TargetEntity.Id == targetEntity?.UniqueId)
				{
					return item.Path;
				}
			}
		}
		using (ProfileScope.New("FindPathChargeTB"))
		{
			FindPath_Blocking(warhammerPathCharge, agent.TurnBasedOptions);
			if (m_ChargePathCache.Count >= 5)
			{
				PathPool.Pool(m_ChargePathCache.Dequeue().Path);
			}
			m_ChargePathCache.Enqueue(new WarhammerPathChargeCacheEntry
			{
				Entity = agent.Unit.Data.Ref,
				Path = warhammerPathCharge,
				Origin = origin,
				Destination = destination,
				IgnoreBlockers = ignoreBlockers,
				TargetEntity = (((EntityRef?)targetEntity?.Ref) ?? default(EntityRef))
			});
			return warhammerPathCharge;
		}
	}

	public Task<PathDisposable<WarhammerPathPlayer>> FindPathTB_Task(UnitMovementAgent agent, Vector3 destination, int maxTiles, object pathOwner)
	{
		WarhammerPathPlayer warhammerPathPlayer = null;
		try
		{
			warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, maxTiles, destination, null, ignoreThreateningAreaCost: false, oneWayLinksAreForbidden: false);
			TaskCompletionSource<PathDisposable<WarhammerPathPlayer>> tcs = new TaskCompletionSource<PathDisposable<WarhammerPathPlayer>>();
			FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, delegate(WarhammerPathPlayer path)
			{
				tcs.SetResult(PathDisposable<WarhammerPathPlayer>.Get(path, pathOwner));
			});
			return tcs.Task;
		}
		catch (Exception ex)
		{
			warhammerPathPlayer?.Claim(this);
			warhammerPathPlayer?.Release(this);
			PFLog.Default.Exception(ex, "Exception during FindPathTbAsync");
			return null;
		}
	}

	public Dictionary<GraphNode, WarhammerPathPlayerCell> FindAllReachableTiles_Blocking(UnitMovementAgent agent, Vector3 start, float maxLength, bool ignoreThreateningAreaCost = false, bool oneWayLinksAreForbidden = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, start, maxLength, null, null, ignoreThreateningAreaCost, oneWayLinksAreForbidden);
		using (ProfileScope.New("AFindPathTB"))
		{
			using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
			{
				FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
				return warhammerPathPlayer.GetAllNodesAndReset();
			}
		}
	}

	public Task<Dictionary<GraphNode, WarhammerPathAiCell>> FindAllReachableTiles_Delayed_Task(UnitMovementAgent agent, GraphNode startNode, int maxTiles, Dictionary<GraphNode, AiBrainHelper.IThreatsInfo> threateningAreaCells, AiThreatsHandlingStrategy threatsHandlingStrategy = AiThreatsHandlingStrategy.AvoidIfPossible)
	{
		TaskCompletionSource<Dictionary<GraphNode, WarhammerPathAiCell>> tcs = new TaskCompletionSource<Dictionary<GraphNode, WarhammerPathAiCell>>();
		if (!UnitMovementAgent.AllAgents.Contains(agent))
		{
			PFLog.Default.Error($"Cant find reachable tiles for disabled UnitMovementAgent: {agent}");
			tcs.SetResult(null);
			return tcs.Task;
		}
		AiBrainHelper.IThreatsInfo threatsData = AiBrainHelper.GetThreatsData((BaseUnitEntity)agent.Unit.Data, startNode);
		WarhammerPathAiMetricCostProvider traversalCostProvider = new WarhammerPathAiMetricCostProvider(agent.Unit.Data, threateningAreaCells, null, threatsHandlingStrategy);
		WarhammerPathAiMetric initialLength = new WarhammerPathAiMetric(agent.Unit.Data.GetCombatStateOptional()?.LastDiagonalCount ?? 0, 0f, 0f, threatsData.AreaEffects.Count, 0, 0, 0);
		WarhammerPathAi warhammerPathAi = ConstructWarhammerPathAi(agent, startNode.Vector3Position(), maxTiles, initialLength, traversalCostProvider);
		warhammerPathAi.Claim(this);
		Action<Path> item = delegate(Path p)
		{
			Dictionary<GraphNode, WarhammerPathAiCell> result = ((p != null && !p.error && p is WarhammerPathAi warhammerPathAi2) ? warhammerPathAi2.GetAllNodesAndReset() : null);
			tcs.SetResult(result);
		};
		FindPathWithType(warhammerPathAi, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathAi, callback: item, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + 1));
		return tcs.Task;
	}

	private void AddDelayedPath((Path path, Action<Path> callback, int delayToStep) value)
	{
		int index = 0;
		int num = m_DelayedPaths.Count - 1;
		while (0 <= num)
		{
			if (m_DelayedPaths[num].delayToStep <= value.delayToStep)
			{
				index = num + 1;
				break;
			}
			num--;
		}
		m_DelayedPaths.Insert(index, value);
	}
}
