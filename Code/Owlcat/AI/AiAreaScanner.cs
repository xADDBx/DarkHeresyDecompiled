using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI;

public class AiAreaScanner
{
	public struct PathData
	{
		public Dictionary<GraphNode, WarhammerPathAiCell> cells;

		public WarhammerPathAiCell startCell;

		public static PathData Zero
		{
			get
			{
				PathData result = default(PathData);
				result.cells = null;
				return result;
			}
		}

		public bool IsZero => cells == null;
	}

	public static async Task<PathData> FindAllReachableNodesAsync(BaseUnitEntity unit, Vector3 pos, float maxPathLen, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (!unit.CanMove)
		{
			PFLog.AI.Log($"Unit {unit} cant move, find nodes skipped");
			return PathData.Zero;
		}
		GridNodeBase startNode = (GridNodeBase)AstarPath.active.GetNearest(pos).node;
		if (startNode == null)
		{
			return PathData.Zero;
		}
		Dictionary<GraphNode, AiBrainHelper.IThreatsInfo> threateningAreaCells = GetThreateningAreaCells(unit, threatsHandlingStrategy);
		Dictionary<GraphNode, WarhammerPathAiCell> dictionary = await PathfindingService.Instance.FindAllReachableTiles_Delayed_Task(unit.View.MovementAgent, startNode, (int)maxPathLen, threateningAreaCells);
		if (dictionary == null)
		{
			PFLog.AI.Error($"WarhammerPath result is null for unit {unit}");
			return PathData.Zero;
		}
		if (!dictionary.ContainsKey(startNode))
		{
			PFLog.AI.Error($"WarhammerPath result is weird: unit={unit}, startPos={pos}, startNode={startNode}, result.Count={dictionary.Count}");
			foreach (var (arg, warhammerPathAiCell2) in dictionary)
			{
				PFLog.AI.Log($"Node: {arg}, PathNode: {warhammerPathAiCell2}");
			}
			return PathData.Zero;
		}
		PathData result = default(PathData);
		result.cells = dictionary;
		result.startCell = dictionary[startNode];
		return result;
	}

	private static Dictionary<GraphNode, AiBrainHelper.IThreatsInfo> GetThreateningAreaCells(BaseUnitEntity unit, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		if (threatsHandlingStrategy == AiThreatsHandlingStrategy.AvoidIfPossible)
		{
			return AiBrainHelper.GatherThreatsData(unit);
		}
		return new Dictionary<GraphNode, AiBrainHelper.IThreatsInfo>();
	}

	public static async Task<PathData> FindAllDeviationNodesAsync(BaseUnitEntity unit, Vector3 pos, GraphNode targetNode, float maxPathLen)
	{
		if (!unit.CanMove)
		{
			PFLog.AI.Log($"Unit {unit} cant move, find nodes skipped");
			return PathData.Zero;
		}
		GridNodeBase startNode = (GridNodeBase)AstarPath.active.GetNearest(pos).node;
		if (startNode == null)
		{
			return PathData.Zero;
		}
		Dictionary<GraphNode, AiBrainHelper.IThreatsInfo> threateningAreaCells = AiBrainHelper.GatherThreatsData(unit);
		Dictionary<GraphNode, WarhammerPathAiCell> dictionary = await PathfindingService.Instance.FindAllDeviationNodesForTargetNode(unit.View.MovementAgent, pos, targetNode, (int)maxPathLen, threateningAreaCells);
		if (dictionary == null)
		{
			PFLog.AI.Error($"WarhammerPath result is null for unit {unit}");
			return PathData.Zero;
		}
		if (!dictionary.ContainsKey(startNode))
		{
			PFLog.AI.Error($"WarhammerPath result is weird: unit={unit}, startPos={pos}, startNode={startNode}, result.Count={dictionary.Count}");
			foreach (var (arg, warhammerPathAiCell2) in dictionary)
			{
				PFLog.AI.Log($"Node: {arg}, PathNode: {warhammerPathAiCell2}");
			}
			return PathData.Zero;
		}
		PathData result = default(PathData);
		result.cells = dictionary;
		result.startCell = dictionary[startNode];
		return result;
	}
}
