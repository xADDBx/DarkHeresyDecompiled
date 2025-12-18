using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI;

public class MoveToEntityNode : TaskNode
{
	private struct PathData
	{
		public ForcedPath Path;

		public RuleCalculateMovementCost Cost;

		public int ThreatFactor;
	}

	private readonly EntityVariable m_Agent;

	private readonly EntityVariable m_Target;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private readonly AiThreatsHandlingStrategy m_ThreatsHandlingStrategy;

	private readonly PreferredPositionNearEntity m_PreferredPositionNearEntity;

	private Task<bool> m_ProcessMovementTask;

	public MoveToEntityNode(EntityVariable agent, EntityVariable target, AiAgentRuntimeInternalDataVariable runtimeInternalData, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		m_Agent = agent;
		m_Target = target;
		m_RuntimeInternalData = runtimeInternalData;
		m_ThreatsHandlingStrategy = threatsHandlingStrategy;
		m_PreferredPositionNearEntity = preferredPositionNearEntity;
	}

	protected override NodeResult OnRunningTick()
	{
		if (m_ProcessMovementTask == null)
		{
			m_ProcessMovementTask = ProcessMovementAsync((BaseUnitEntity)m_Agent.Value, m_Target.Value, m_RuntimeInternalData.Value, m_ThreatsHandlingStrategy, m_PreferredPositionNearEntity);
		}
		if (!m_ProcessMovementTask.IsCompleted)
		{
			return NodeResult.Running;
		}
		bool flag;
		try
		{
			flag = m_ProcessMovementTask.Result;
		}
		catch (Exception ex)
		{
			PFLog.AI.Exception(ex);
			flag = false;
		}
		m_ProcessMovementTask = null;
		if (!flag)
		{
			return NodeResult.Failure;
		}
		return NodeResult.Success;
	}

	private async Task<bool> ProcessMovementAsync(BaseUnitEntity agent, MechanicEntity target, AiAgentRuntimeInternalData runtimeData, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		if (target == null)
		{
			PFLog.AI.Error("Target is null");
			return false;
		}
		if (threatsHandlingStrategy != 0)
		{
			await runtimeData.UpdateMoveVariants(agent, threatsHandlingStrategy);
		}
		UnitMoveToProperParams commandParams = await TryCreateMoveCommandAsync(agent, target, runtimeData.AgentMoveVariants, threatsHandlingStrategy, preferredPositionNearEntity);
		if (commandParams == null)
		{
			PFLog.AI.Log("Move command was not set up -> already moved");
			return true;
		}
		if (agent.IsInSquad)
		{
			agent.GetSquadOptional()?.Squad.Data.TryReserve(commandParams.ForcedPath.path.Last());
		}
		while (!agent.Commands.Empty)
		{
			await Task.Delay(100);
		}
		PFLog.AI.Log($"Try move to {commandParams.ForcedPath.path.Last()}");
		UnitCommandHandle commandHandle = agent.Commands.RunImmediate(commandParams);
		if (commandHandle == null)
		{
			return false;
		}
		while (!commandHandle.IsFinished)
		{
			runtimeData.ResetIdleTime();
			await Task.Delay(100);
		}
		runtimeData.Invalidate();
		return true;
	}

	private async Task<UnitMoveToProperParams> TryCreateMoveCommandAsync(BaseUnitEntity agent, MechanicEntity target, AiAreaScanner.PathData moveVariants, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		ForcedPath forcedPath = await CreatePathToTargetAsync(agent, target, moveVariants, threatsHandlingStrategy, preferredPositionNearEntity);
		if (forcedPath == null)
		{
			return null;
		}
		RuleCalculateMovementCost ruleCalculateMovementCost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath));
		int num = ruleCalculateMovementCost.ResultPointCount;
		while (num > 0)
		{
			GraphNode graphNode = forcedPath.path[num - 1];
			if (CanStopAtNode(agent, graphNode, moveVariants))
			{
				break;
			}
			num--;
			PFLog.AI.Log($"{graphNode} is unreachable, trim path");
		}
		if (num < 2)
		{
			forcedPath.Release(this);
			return null;
		}
		float[] resultAPCostPerPoint = ruleCalculateMovementCost.ResultAPCostPerPoint;
		ForcedPath path = ForcedPath.Construct(forcedPath.vectorPath.Take(num), forcedPath.path.Take(num));
		forcedPath.Release(this);
		return new UnitMoveToProperParams(path, agent.Blueprint.WarhammerMovementApPerCell, resultAPCostPerPoint);
	}

	private async Task<ForcedPath> CreatePathToTargetAsync(BaseUnitEntity agent, MechanicEntity target, AiAreaScanner.PathData moveVariants, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		List<PathData> paths = new List<PathData>();
		ForcedPath path = null;
		try
		{
			if (moveVariants.IsZero)
			{
				return null;
			}
			AiAreaScanner.PathData pathData = moveVariants;
			HashSet<GridNodeBase> nodesNearTargetEntity = GetNodesNearTargetEntity(agent, target, preferredPositionNearEntity);
			CollectPathsToTargetNodes(agent, nodesNearTargetEntity, pathData, ref paths);
			if (paths.Count == 0)
			{
				pathData = await AiAreaScanner.FindAllReachableNodesAsync(agent, agent.Position, 50f, threatsHandlingStrategy);
				CollectPathsToTargetNodes(agent, nodesNearTargetEntity, pathData, ref paths);
			}
			PathData pathData2 = paths.MinBy((PathData p) => (float)(p.ThreatFactor * 100) + p.Cost.ResultFullPathAPCost);
			if (pathData2.Path == null || pathData2.Cost == null)
			{
				var (gridNodeBase, fromSize) = GetEntityNodeAndSize(target);
				if (gridNodeBase == null)
				{
					return null;
				}
				GraphNode graphNode = null;
				int num = int.MaxValue;
				foreach (KeyValuePair<GraphNode, WarhammerPathAiCell> item in pathData.cells.Where((KeyValuePair<GraphNode, WarhammerPathAiCell> pair) => CanStopAtNode(agent, pair.Key, pathData)))
				{
					int num2 = WarhammerGeometryUtils.DistanceToInCells(gridNodeBase.Vector3Position(), fromSize, item.Key.Vector3Position(), agent.SizeRect);
					if (num2 < num)
					{
						num = num2;
						graphNode = item.Key;
					}
				}
				if (graphNode == null)
				{
					return null;
				}
				ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(graphNode, pathData.cells);
				forcedPath.Claim(this);
				RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath, calcFullPathApCost: true));
				PathData pathData3 = default(PathData);
				pathData3.Path = forcedPath;
				pathData3.Cost = cost;
				pathData2 = pathData3;
			}
			path = pathData2.Path;
			return path;
		}
		finally
		{
			paths?.Where((PathData p) => p.Path != path).ForEach(delegate(PathData p)
			{
				p.Path?.Release(this);
			});
		}
	}

	private void CollectPathsToTargetNodes(BaseUnitEntity agent, HashSet<GridNodeBase> targetNodes, AiAreaScanner.PathData moveVariants, ref List<PathData> collectedPaths)
	{
		foreach (GridNodeBase targetNode in targetNodes)
		{
			if (moveVariants.cells.TryGetValue(targetNode, out var value) && CanStopAtNode(agent, value.Node, moveVariants))
			{
				ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(targetNode, moveVariants.cells);
				forcedPath.Claim(this);
				RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath, calcFullPathApCost: true));
				int threatFactor = value.EnteredAoE - value.LeavedAoE + value.StepsInsideDamagingAoE + value.ProvokedAttacks;
				collectedPaths.Add(new PathData
				{
					Path = forcedPath,
					Cost = cost,
					ThreatFactor = threatFactor
				});
			}
		}
	}

	private HashSet<GridNodeBase> GetNodesNearTargetEntity(BaseUnitEntity agent, MechanicEntity targetEntity, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		HashSet<GridNodeBase> hashSet = EnumerateNodesNearEntity(targetEntity, preferredPositionNearEntity).ToHashSet();
		IntRect sizeRect = agent.SizeRect;
		if (sizeRect.Width == 1 && sizeRect.Height == 1)
		{
			return hashSet;
		}
		if (hashSet.Count == 0)
		{
			return hashSet;
		}
		GridGraph gridGraph = hashSet.First().Graph as GridGraph;
		foreach (GridNodeBase item in hashSet.ToTempList())
		{
			for (int i = 0; i < sizeRect.Width; i++)
			{
				for (int j = 0; j < sizeRect.Height; j++)
				{
					hashSet.Add(gridGraph.GetNode(item.XCoordinateInGrid - i, item.ZCoordinateInGrid - j));
				}
			}
		}
		return hashSet;
	}

	private IEnumerable<GridNodeBase> EnumerateNodesNearEntity(MechanicEntity entity, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		return EnumerateNodesNearEntity(entity.GetNearestNodeXZ(), entity.SizeRect, preferredPositionNearEntity);
	}

	private IEnumerable<GridNodeBase> EnumerateNodesNearEntity(GridNodeBase node, IntRect sizeRect, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		List<GridNodeBase> entityNodes = GridAreaHelper.GetOccupiedNodes(node, sizeRect).ToList();
		int xFrom = entityNodes.Min((GridNodeBase n) => n.XCoordinateInGrid) - 1;
		int xTo = entityNodes.Max((GridNodeBase n) => n.XCoordinateInGrid) + 1;
		int zFrom = entityNodes.Min((GridNodeBase n) => n.ZCoordinateInGrid) - 1;
		int zTo = entityNodes.Max((GridNodeBase n) => n.ZCoordinateInGrid) + 1;
		GridGraph graph = entityNodes[0].Graph as GridGraph;
		foreach (GridNodeBase entityNode in entityNodes)
		{
			for (int x = xFrom; x <= xTo; x++)
			{
				for (int z = zFrom; z <= zTo; z++)
				{
					bool flag = (x == xFrom || x == xTo) && (z == zFrom || z == zTo);
					if (!(preferredPositionNearEntity == PreferredPositionNearEntity.AdjacentGraphNodes && flag) && (preferredPositionNearEntity != PreferredPositionNearEntity.DiagonalGraphNodes || flag))
					{
						GridNodeBase node2 = graph.GetNode(x, z);
						if (node2 != null && !entityNodes.Contains(node2) && entityNode.ContainsConnection(node2))
						{
							yield return node2;
						}
					}
				}
			}
		}
	}

	private bool CanStopAtNode(BaseUnitEntity agent, GraphNode node, AiAreaScanner.PathData moveVariants)
	{
		agent.GetNearestNodeXZ();
		if (agent.IsInSquad)
		{
			PartSquad squadOptional = agent.GetSquadOptional();
			if (squadOptional != null && squadOptional.Squad.Data.IsReserved(node))
			{
				return false;
			}
		}
		if (moveVariants.cells.TryGetValue(node, out var value))
		{
			return value.IsCanStand;
		}
		return false;
	}

	private (GridNodeBase, IntRect) GetEntityNodeAndSize(MechanicEntity entity)
	{
		return (entity.GetNearestNodeXZ(), entity.SizeRect);
	}
}
