using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public static class BehaviourTreeRuntimeToBlueprintBridgeExtensions
{
	public static void SetAgentVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, MechanicEntity agent)
	{
		runtimeBridge.Blackboard.GetAgentVariable().Value = agent;
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetAgentVariable(agent);
		}
	}

	public static void SetUnitsInCombatVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, List<MechanicEntity> unitsInCombat)
	{
		runtimeBridge.Blackboard.GetUnitsInCombatVariable().Value = unitsInCombat;
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetUnitsInCombatVariable(unitsInCombat);
		}
	}

	public static void SetAlliesInCombatVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, List<MechanicEntity> alliesInCombat)
	{
		runtimeBridge.Blackboard.GetAlliesInCombatVariable().Value = alliesInCombat;
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetAlliesInCombatVariable(alliesInCombat);
		}
	}

	public static void SetEnemiesInCombatVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, List<MechanicEntity> enemiesInCombat)
	{
		runtimeBridge.Blackboard.GetEnemiesInCombatVariable().Value = enemiesInCombat;
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetEnemiesInCombatVariable(enemiesInCombat);
		}
	}

	public static void SetReachableNodesVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, IEnumerable<GraphNode> reachableNodes)
	{
		runtimeBridge.Blackboard.GetReachableNodesVariable().Value = reachableNodes.ToList();
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetReachableNodesVariable(reachableNodes);
		}
	}

	public static void SetRuntimeInternalDataVariable(this BehaviourTreeRuntimeToBlueprintBridge runtimeBridge, AiAgentRuntimeInternalData runtimeInternalData)
	{
		runtimeBridge.Blackboard.GetRuntimeInternalDataVariable().Value = runtimeInternalData;
		foreach (SubTreeNode subTreeNode in runtimeBridge.BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.SetRuntimeInternalDataVariable(runtimeInternalData);
		}
	}
}
