using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Squads.Goals;
using Owlcat.AI;

namespace Kingmaker.UnitLogic.Squads;

public static class SquadPlanConsumer
{
	public static async Task<UnitMoveToProperParams> TryConsumeAsync(IPathClaimer claimer, BaseUnitEntity agent, Func<IMovementGoal> goalFactory, AiThreatsHandlingStrategy threats)
	{
		if (agent == null || goalFactory == null)
		{
			return null;
		}
		if (!agent.IsInSquad)
		{
			return null;
		}
		UnitSquad squad = agent.GetSquadOptional()?.Squad;
		if (squad == null || squad.AliveUnitsCount <= 1)
		{
			return null;
		}
		SquadMovementPlanner.EnsureMainThread();
		Task<SquadMovementPlan> task = squad.Data.TryStakeMovementPlanTask(delegate
		{
			IMovementGoal movementGoal = goalFactory();
			if (movementGoal == null)
			{
				return (Task<SquadMovementPlan>)null;
			}
			float squadMovementCostWeight = ConfigRoot.Instance.CombatRoot.SquadMovementCostWeight;
			CancellationToken movementPlanToken = squad.Data.MovementPlanToken;
			return SquadMovementPlanner.BuildAsync(squad, movementGoal, squadMovementCostWeight, threats, movementPlanToken);
		});
		if (task == null)
		{
			return null;
		}
		SquadMovementPlan squadMovementPlan;
		try
		{
			squadMovementPlan = await task;
		}
		catch (OperationCanceledException)
		{
			return null;
		}
		catch (Exception ex2)
		{
			PFLog.AI.Exception(ex2, $"Squad movement planner failed for {squad}; falling back to per-unit path selection.");
			return null;
		}
		if (!squadMovementPlan.Assignments.TryGetValue(agent, out var value))
		{
			return null;
		}
		ForcedPath path = value.Path;
		if (path == null || path.path == null || path.path.Count < 2)
		{
			return null;
		}
		path.Claim(claimer);
		RuleCalculateMovementCost ruleCalculateMovementCost = Rulebook.Trigger(new RuleCalculateMovementCost(agent, path));
		int resultPointCount = ruleCalculateMovementCost.ResultPointCount;
		if (resultPointCount < 2)
		{
			path.Release(claimer);
			return null;
		}
		float[] resultAPCostPerPoint = ruleCalculateMovementCost.ResultAPCostPerPoint;
		ForcedPath path2 = ForcedPath.Construct(path.vectorPath.Take(resultPointCount), path.path.Take(resultPointCount));
		path.Release(claimer);
		squad.Data.TryReserve(value.TargetCell);
		return new UnitMoveToProperParams(path2, agent.Blueprint.WarhammerMovementApPerCell, resultAPCostPerPoint);
	}
}
