using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Code.Framework.Networking.Sync;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.UnitLogic.Squads.Goals;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class MoveToEntityNode : TaskNode, IPathClaimer
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

	private readonly SquadGoalKind m_SquadGoalKind;

	private Task<bool> m_ProcessMovementTask;

	public MoveToEntityNode(EntityVariable agent, EntityVariable target, AiAgentRuntimeInternalDataVariable runtimeInternalData, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity, SquadGoalKind squadGoalKind = SquadGoalKind.None)
	{
		m_Agent = agent;
		m_Target = target;
		m_RuntimeInternalData = runtimeInternalData;
		m_ThreatsHandlingStrategy = threatsHandlingStrategy;
		m_PreferredPositionNearEntity = preferredPositionNearEntity;
		m_SquadGoalKind = squadGoalKind;
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
		UnitMoveToProperParams commandParams;
		await using (AsyncSimulationScope.Get())
		{
			if (threatsHandlingStrategy == AiThreatsHandlingStrategy.Ignore)
			{
				await runtimeData.UpdateMoveVariants(agent, threatsHandlingStrategy);
			}
			commandParams = await TryCreateMoveCommandAsync(agent, target, runtimeData.AgentMoveVariants, threatsHandlingStrategy, preferredPositionNearEntity);
		}
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
			await NextTickAwaiter.New();
		}
		PFLog.AI.Log("Try move to " + commandParams.ForcedPath.path.Last().AsString());
		UnitCommandHandle commandHandle = agent.Commands.RunImmediate(commandParams);
		if (commandHandle == null)
		{
			return false;
		}
		await using (AsyncSimulationScope.Get())
		{
			while (!commandHandle.IsFinished)
			{
				runtimeData.ResetIdleTime();
				await NextTickAwaiter.New();
			}
		}
		runtimeData.Invalidate();
		return true;
	}

	private async Task<UnitMoveToProperParams> TryCreateMoveCommandAsync(BaseUnitEntity agent, MechanicEntity target, AiAreaScanner.PathData moveVariants, AiThreatsHandlingStrategy threatsHandlingStrategy, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		UnitMoveToProperParams unitMoveToProperParams = await TryCreateCommandFromSquadPlanAsync(agent, target, preferredPositionNearEntity, threatsHandlingStrategy);
		if (unitMoveToProperParams != null)
		{
			return unitMoveToProperParams;
		}
		ForcedPath forcedPath = await CreatePathToTargetAsync(agent, target, moveVariants, threatsHandlingStrategy, preferredPositionNearEntity);
		if (forcedPath == null)
		{
			return null;
		}
		RuleCalculateMovementCost ruleCalculateMovementCost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath));
		int num = ruleCalculateMovementCost.ResultPointCount;
		while (num > 0)
		{
			GraphNode node = forcedPath.path[num - 1];
			if (CanStopAtNode(agent, node, moveVariants))
			{
				break;
			}
			num--;
			PFLog.AI.Log(node.AsString() + " is unreachable, trim path");
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

	private async Task<UnitMoveToProperParams> TryCreateCommandFromSquadPlanAsync(BaseUnitEntity agent, MechanicEntity target, PreferredPositionNearEntity preferredPositionNearEntity, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (m_SquadGoalKind == SquadGoalKind.None)
		{
			return null;
		}
		return await SquadPlanConsumer.TryConsumeAsync(this, agent, GoalFactory, threatsHandlingStrategy);
		IMovementGoal GoalFactory()
		{
			if (m_SquadGoalKind == SquadGoalKind.MeleeAttack)
			{
				return new MeleeAttackGoal(target, preferredPositionNearEntity);
			}
			return null;
		}
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
		return MeleeCandidateHelper.GetNodesNearTargetEntity(agent, targetEntity, preferredPositionNearEntity);
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
