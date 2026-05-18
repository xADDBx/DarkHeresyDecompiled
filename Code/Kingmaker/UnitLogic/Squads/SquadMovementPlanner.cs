using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Squads.Goals;
using Owlcat.AI;
using Pathfinding;

namespace Kingmaker.UnitLogic.Squads;

public static class SquadMovementPlanner
{
	private struct PerUnitReach
	{
		public Dictionary<int, (ForcedPath Path, float Cost)> CellCost;
	}

	private static readonly int s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

	internal static void EnsureMainThread()
	{
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		if (managedThreadId != s_MainThreadId)
		{
			PFLog.AI.Error($"SquadMovementPlanner accessed from thread {managedThreadId}; expected main thread {s_MainThreadId}. " + "MovementPlanTask memoization is not race-safe on worker threads.");
		}
	}

	public static async Task<SquadMovementPlan> BuildAsync(UnitSquad squad, IMovementGoal goal, float costWeight, AiThreatsHandlingStrategy threats, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (squad == null)
		{
			throw new ArgumentNullException("squad");
		}
		if (goal == null)
		{
			throw new ArgumentNullException("goal");
		}
		cancellationToken.ThrowIfCancellationRequested();
		IReadOnlyList<SquadCandidateCell> candidates = goal.GetCandidates(squad);
		if (candidates == null || candidates.Count == 0)
		{
			return new SquadMovementPlan();
		}
		List<BaseUnitEntity> units = new List<BaseUnitEntity>();
		foreach (UnitReference unit in squad.Units)
		{
			BaseUnitEntity baseUnitEntity = unit.ToBaseUnitEntity();
			if (baseUnitEntity != null && !baseUnitEntity.IsDeadOrUnconscious && baseUnitEntity.CanAct && baseUnitEntity.CanMove)
			{
				units.Add(baseUnitEntity);
			}
		}
		if (units.Count == 0)
		{
			return new SquadMovementPlan();
		}
		ForcedPath[,] paths = null;
		try
		{
			Task<PerUnitReach>[] array = new Task<PerUnitReach>[units.Count];
			for (int i = 0; i < units.Count; i++)
			{
				array[i] = CollectReachabilityForUnitAsync(units[i], candidates, threats);
			}
			PerUnitReach[] array2 = await Task.WhenAll(array);
			cancellationToken.ThrowIfCancellationRequested();
			int count = units.Count;
			int count2 = candidates.Count;
			float[] array3 = new float[count2];
			for (int j = 0; j < count2; j++)
			{
				array3[j] = candidates[j].Desirability;
			}
			float[,] array4 = new float[count, count2];
			paths = new ForcedPath[count, count2];
			for (int k = 0; k < count; k++)
			{
				PerUnitReach perUnitReach = array2[k];
				for (int l = 0; l < count2; l++)
				{
					if (perUnitReach.CellCost.TryGetValue(l, out (ForcedPath, float) value))
					{
						array4[k, l] = value.Item2;
						paths[k, l] = value.Item1;
					}
					else
					{
						array4[k, l] = float.PositiveInfinity;
						paths[k, l] = null;
					}
				}
			}
			int[] array5 = GreedyAssign(count, count2, array3, array4, costWeight);
			SquadMovementPlan squadMovementPlan = new SquadMovementPlan();
			for (int m = 0; m < count; m++)
			{
				int num = array5[m];
				if (num >= 0)
				{
					squadMovementPlan.Assignments[units[m]] = new SquadMovementAssignment(candidates[num].Node, paths[m, num], array4[m, num]);
					paths[m, num] = null;
				}
			}
			ReleaseAll(paths);
			paths = null;
			return squadMovementPlan;
		}
		catch (OperationCanceledException)
		{
			ReleaseAll(paths);
			throw;
		}
	}

	private static void ReleaseAll(ForcedPath[,] paths)
	{
		if (paths == null)
		{
			return;
		}
		for (int i = 0; i < paths.GetLength(0); i++)
		{
			for (int j = 0; j < paths.GetLength(1); j++)
			{
				paths[i, j]?.Release(SquadMovementPlan.PoolClaimer);
			}
		}
	}

	public static int[] GreedyAssign(int unitCount, int cellCount, float[] desirabilities, float[,] costs, float costWeight)
	{
		int[] array = new int[unitCount];
		for (int i = 0; i < unitCount; i++)
		{
			array[i] = -1;
		}
		List<(int, int, float)> list = new List<(int, int, float)>(unitCount * cellCount);
		for (int j = 0; j < unitCount; j++)
		{
			for (int k = 0; k < cellCount; k++)
			{
				float num = costs[j, k];
				if (!float.IsInfinity(num) && !float.IsNaN(num))
				{
					float item = desirabilities[k] - costWeight * num;
					list.Add((j, k, item));
				}
			}
		}
		list.Sort(delegate((int unit, int cell, float score) a, (int unit, int cell, float score) b)
		{
			int num3 = b.score.CompareTo(a.score);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = a.unit.CompareTo(b.unit);
			return (num4 != 0) ? num4 : a.cell.CompareTo(b.cell);
		});
		bool[] array2 = new bool[cellCount];
		int num2 = 0;
		foreach (var item2 in list)
		{
			if (num2 == unitCount)
			{
				break;
			}
			if (array[item2.Item1] == -1 && !array2[item2.Item2])
			{
				array[item2.Item1] = item2.Item2;
				array2[item2.Item2] = true;
				num2++;
			}
		}
		return array;
	}

	private static async Task<PerUnitReach> CollectReachabilityForUnitAsync(BaseUnitEntity unit, IReadOnlyList<SquadCandidateCell> candidates, AiThreatsHandlingStrategy threats)
	{
		PerUnitReach result = new PerUnitReach
		{
			CellCost = new Dictionary<int, (ForcedPath, float)>()
		};
		PartUnitCombatState combatStateOptional = unit.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return result;
		}
		float movementPoints = combatStateOptional.MovementPoints;
		if (movementPoints <= 0f)
		{
			return result;
		}
		AiAreaScanner.PathData pathData;
		try
		{
			pathData = await AiAreaScanner.FindAllReachableNodesAsync(unit, unit.Position, movementPoints, threats);
		}
		catch (Exception ex)
		{
			PFLog.AI.Exception(ex, $"AiAreaScanner failed for {unit} during squad plan; unit will not get an assignment this turn.");
			return result;
		}
		if (pathData.IsZero || pathData.cells == null)
		{
			return result;
		}
		for (int i = 0; i < candidates.Count; i++)
		{
			GraphNode node = candidates[i].Node;
			if (node == null || !pathData.cells.ContainsKey(node))
			{
				continue;
			}
			ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(node, pathData.cells);
			if (forcedPath != null)
			{
				forcedPath.Claim(SquadMovementPlan.PoolClaimer);
				float resultFullPathAPCost = Rulebook.Trigger(new RuleCalculateMovementCost(unit, forcedPath, calcFullPathApCost: true)).ResultFullPathAPCost;
				if (float.IsNaN(resultFullPathAPCost))
				{
					forcedPath.Release(SquadMovementPlan.PoolClaimer);
				}
				else
				{
					result.CellCost[i] = (forcedPath, resultFullPathAPCost);
				}
			}
		}
		return result;
	}
}
