using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class MoveToInteractableNode : TaskNode
{
	private struct PathData
	{
		public ForcedPath Path;

		public RuleCalculateMovementCost Cost;

		public int ThreatFactor;
	}

	private readonly EntityVariable m_Agent;

	private readonly InteractableVariable m_Interactable;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private readonly AiThreatsHandlingStrategy m_ThreatsHandlingStrategy;

	private Task<bool> m_ProcessMovementTask;

	public MoveToInteractableNode(EntityVariable agent, InteractableVariable interactable, AiAgentRuntimeInternalDataVariable runtimeInternalData, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		m_Agent = agent;
		m_Interactable = interactable;
		m_RuntimeInternalData = runtimeInternalData;
		m_ThreatsHandlingStrategy = threatsHandlingStrategy;
	}

	protected override NodeResult OnRunningTick()
	{
		if (m_ProcessMovementTask == null)
		{
			m_ProcessMovementTask = ProcessMovementAsync((BaseUnitEntity)m_Agent.Value, m_Interactable.Value, m_RuntimeInternalData.Value, m_ThreatsHandlingStrategy);
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

	private async Task<bool> ProcessMovementAsync(BaseUnitEntity agent, InteractionAction interactable, AiAgentRuntimeInternalData runtimeData, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (interactable == null)
		{
			PFLog.AI.Error("Interactable is null");
			return false;
		}
		if (threatsHandlingStrategy != 0)
		{
			await runtimeData.UpdateMoveVariants(agent, threatsHandlingStrategy);
		}
		UnitMoveToProperParams commandParams = await TryCreateMoveCommandAsync(agent, interactable, runtimeData.AgentMoveVariants, threatsHandlingStrategy);
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

	private async Task<UnitMoveToProperParams> TryCreateMoveCommandAsync(BaseUnitEntity agent, InteractionAction interactable, AiAreaScanner.PathData moveVariants, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		ForcedPath forcedPath = await CreatePathToInteractableAsync(agent, interactable, moveVariants, threatsHandlingStrategy);
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

	private async Task<ForcedPath> CreatePathToInteractableAsync(BaseUnitEntity agent, InteractionAction interactable, AiAreaScanner.PathData moveVariants, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		List<PathData> paths = new List<PathData>();
		ForcedPath path = null;
		try
		{
			if (moveVariants.IsZero)
			{
				return null;
			}
			HashSet<GridNodeBase> interactableArea = GetInteractableArea(interactable);
			if (interactableArea.Contains(agent.GetNearestNodeXZ()))
			{
				return null;
			}
			CollectPathsToTargetNodes(agent, interactableArea, moveVariants, ref paths);
			if (paths.Count == 0)
			{
				AiAreaScanner.PathData moveVariants2 = await AiAreaScanner.FindAllReachableNodesAsync(agent, agent.Position, 50f, threatsHandlingStrategy);
				CollectPathsToTargetNodes(agent, interactableArea, moveVariants2, ref paths);
			}
			PathData pathData = paths.MinBy((PathData p) => (float)(p.ThreatFactor * 100) + p.Cost.ResultFullPathAPCost);
			if (pathData.Path == null || pathData.Cost == null)
			{
				GridNodeBase gridNodeBase = interactableArea.FirstOrDefault();
				if (gridNodeBase == null)
				{
					return null;
				}
				GraphNode graphNode = null;
				int num = int.MaxValue;
				foreach (KeyValuePair<GraphNode, WarhammerPathAiCell> item in moveVariants.cells.Where((KeyValuePair<GraphNode, WarhammerPathAiCell> pair) => CanStopAtNode(agent, pair.Key, moveVariants)))
				{
					int num2 = WarhammerGeometryUtils.DistanceToInCells(gridNodeBase.Vector3Position(), default(IntRect), item.Key.Vector3Position(), agent.SizeRect);
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
				ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(graphNode, moveVariants.cells);
				forcedPath.Claim(this);
				RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath, calcFullPathApCost: true));
				PathData pathData2 = default(PathData);
				pathData2.Path = forcedPath;
				pathData2.Cost = cost;
				pathData = pathData2;
			}
			path = pathData.Path;
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

	private void CollectPathsToTargetNodes(BaseUnitEntity agent, IEnumerable<GridNodeBase> targetNodes, AiAreaScanner.PathData moveVariants, ref List<PathData> collectedPaths)
	{
		foreach (GridNodeBase targetNode in targetNodes)
		{
			if (moveVariants.cells.TryGetValue(targetNode, out var value) && CanStopAtNode(agent, value.Node, moveVariants))
			{
				ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(targetNode, moveVariants.cells);
				forcedPath.Claim(this);
				RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, forcedPath, calcFullPathApCost: true));
				int threatFactor = value.EnteredAoE + value.StepsInsideDamagingAoE + value.ProvokedAttacks;
				collectedPaths.Add(new PathData
				{
					Path = forcedPath,
					Cost = cost,
					ThreatFactor = threatFactor
				});
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

	[NotNull]
	private HashSet<GridNodeBase> GetInteractableArea(InteractionAction interactable)
	{
		return new HashSet<GridNodeBase> { ObstacleAnalyzer.GetNearestNodeXZUnwalkable(interactable.transform.position) };
	}
}
