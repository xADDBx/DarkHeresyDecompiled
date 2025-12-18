using System;
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
using Owlcat.BehaviourTrees;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI;

public class MoveToGraphNodeNode : TaskNode
{
	private readonly EntityVariable m_Agent;

	private readonly GraphNodeVariable m_TargetNode;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private readonly TargetNodeSelectionPolicy m_TargetNodeSelectionPolicy;

	private readonly AiThreatsHandlingStrategy m_ThreatsHandlingStrategy;

	private Task<bool> m_ProcessMovementTask;

	public MoveToGraphNodeNode(EntityVariable agent, GraphNodeVariable targetNode, AiAgentRuntimeInternalDataVariable runtimeInternalData, TargetNodeSelectionPolicy targetNodeSelectionPolicy, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		m_Agent = agent;
		m_TargetNode = targetNode;
		m_RuntimeInternalData = runtimeInternalData;
		m_TargetNodeSelectionPolicy = targetNodeSelectionPolicy;
		m_ThreatsHandlingStrategy = threatsHandlingStrategy;
	}

	protected override NodeResult OnRunningTick()
	{
		if (m_ProcessMovementTask == null)
		{
			m_ProcessMovementTask = ProcessMovementAsync((BaseUnitEntity)m_Agent.Value, (GridNodeBase)m_TargetNode.Value, m_RuntimeInternalData.Value, m_TargetNodeSelectionPolicy, m_ThreatsHandlingStrategy);
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

	private async Task<bool> ProcessMovementAsync(BaseUnitEntity agent, GridNodeBase targetNode, AiAgentRuntimeInternalData runtimeData, TargetNodeSelectionPolicy targetNodeSelectionPolicy, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (targetNode == null)
		{
			PFLog.AI.Error("Target node is null");
			return false;
		}
		if (agent.GetNearestNodeXZ() == targetNode)
		{
			PFLog.AI.Log("Move command was not set up -> already moved");
			return true;
		}
		if (threatsHandlingStrategy != 0)
		{
			await runtimeData.UpdateMoveVariants(agent, threatsHandlingStrategy);
		}
		UnitMoveToProperParams commandParams = await TryCreateMoveCommandAsync(agent, targetNode, runtimeData.AgentMoveVariants, targetNodeSelectionPolicy, threatsHandlingStrategy);
		if (commandParams == null)
		{
			PFLog.AI.Log("Move command was not set up -> already moved");
			return targetNodeSelectionPolicy == TargetNodeSelectionPolicy.ClosestToTargetGraphNode;
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

	private async Task<UnitMoveToProperParams> TryCreateMoveCommandAsync(BaseUnitEntity agent, GridNodeBase targetNode, AiAreaScanner.PathData moveVariants, TargetNodeSelectionPolicy targetNodeSelectionPolicy, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		ForcedPath forcedPath = await CreatePathToTargetNodeAsync(agent, targetNode, moveVariants, targetNodeSelectionPolicy, threatsHandlingStrategy);
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

	private async Task<ForcedPath> CreatePathToTargetNodeAsync(BaseUnitEntity agent, GridNodeBase targetNode, AiAreaScanner.PathData moveVariants, TargetNodeSelectionPolicy targetNodeSelectionPolicy, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (moveVariants.IsZero)
		{
			return null;
		}
		if (moveVariants.cells.ContainsKey(targetNode))
		{
			return ConstructPathToTargetNode(targetNode, moveVariants);
		}
		AiAreaScanner.PathData pathData = await AiAreaScanner.FindAllReachableNodesAsync(agent, agent.Position, 50f, threatsHandlingStrategy);
		if (!pathData.IsZero && pathData.cells.ContainsKey(targetNode))
		{
			return ConstructPathToTargetNode(targetNode, pathData);
		}
		if (pathData.IsZero || targetNodeSelectionPolicy == TargetNodeSelectionPolicy.ExactTargetGraphNode)
		{
			return null;
		}
		Vector3 to = targetNode.Vector3Position();
		GraphNode nearestNodeXZ = agent.GetNearestNodeXZ();
		GraphNode graphNode = agent.GetNearestNodeXZ();
		int num = WarhammerGeometryUtils.DistanceToInCells(graphNode.Vector3Position(), agent.SizeRect, to, default(IntRect));
		foreach (GraphNode key in pathData.cells.Keys)
		{
			if (CanStopAtNode(agent, key, pathData))
			{
				int num2 = WarhammerGeometryUtils.DistanceToInCells(key.Vector3Position(), agent.SizeRect, to, default(IntRect));
				if (num2 < num)
				{
					num = num2;
					graphNode = key;
				}
			}
		}
		if (graphNode == nearestNodeXZ)
		{
			return null;
		}
		return ConstructPathToTargetNode(graphNode, pathData);
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
		return moveVariants.cells[node].IsCanStand;
	}

	private ForcedPath ConstructPathToTargetNode(GraphNode targetNode, AiAreaScanner.PathData pathData)
	{
		ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(targetNode, pathData.cells);
		forcedPath.Claim(this);
		return forcedPath;
	}
}
