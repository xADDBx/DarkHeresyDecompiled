using System;
using Kingmaker;
using Kingmaker.UnitLogic.Squads;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SquadControlNode : CompositeNode
{
	private enum ExecutionState
	{
		Initial,
		PlanningBranchFinished,
		PlanningStageFinished,
		SynchronousBranchFinished,
		SquadMemberTurnStarted,
		IndividualBranchFinished
	}

	private class WaitPlanningStageFinished : TaskNode
	{
		protected override NodeResult OnRunningTick()
		{
			if (!(CurrentSquadTurnControlData ?? throw new Exception("SquadControlData.Current is NULL!")).IsAllReady)
			{
				return NodeResult.Running;
			}
			return NodeResult.Success;
		}
	}

	private class WaitSquadMemberTurnStart : TaskNode
	{
		private readonly EntityVariable m_Agent;

		private bool IsSquadMemberTurn => Game.Instance.Controllers.TurnController.CurrentUnit == m_Agent.Value;

		public WaitSquadMemberTurnStart(EntityVariable agent)
		{
			m_Agent = agent;
		}

		protected override NodeResult OnRunningTick()
		{
			if (!IsSquadMemberTurn)
			{
				return NodeResult.Running;
			}
			return NodeResult.Success;
		}
	}

	private readonly EntityVariable m_Agent;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalData;

	private readonly BehaviourTreeNode m_WaitPlanningStageFinished;

	private readonly BehaviourTreeNode m_WaitSquadMemberTurnStart;

	private ExecutionState m_CurrentExecutionState;

	private BehaviourTreeNode PlanningBranchRoot => base.Children[0];

	private BehaviourTreeNode SynchronousBranchRoot => base.Children[1];

	private BehaviourTreeNode IndividualBranchRoot => base.Children[2];

	private static UnitSquad.SquadTurnControlData CurrentSquadTurnControlData => (Game.Instance.Controllers.TurnController.CurrentUnit as UnitSquad)?.Data;

	private static bool IsSquadTurn => Game.Instance.Controllers.TurnController.CurrentUnit is UnitSquad;

	public SquadControlNode(EntityVariable agent, AiAgentRuntimeInternalDataVariable runtimeInternalData)
	{
		m_Agent = agent;
		m_RuntimeInternalData = runtimeInternalData;
		m_WaitPlanningStageFinished = new WaitPlanningStageFinished
		{
			Parent = this
		};
		m_WaitSquadMemberTurnStart = new WaitSquadMemberTurnStart(m_Agent)
		{
			Parent = this
		};
	}

	public override NodeVisitResult ForwardVisit()
	{
		if (base.Children == null || base.Children.Count != 3)
		{
			throw new Exception(GetType().Name + " with title '" + base.Title + "' must have exact 3 children");
		}
		if (!IsSquadTurn)
		{
			throw new Exception("SquadControl node must be used only for units in squads");
		}
		return StartPlanningStage();
	}

	private NodeVisitResult StartPlanningStage()
	{
		if (CurrentSquadTurnControlData == null)
		{
			throw new Exception("SquadControlData.Current is NULL!");
		}
		m_CurrentExecutionState = ExecutionState.Initial;
		return NodeVisitResult.GoForward(PlanningBranchRoot);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		TryUpdateExecutionState(result);
		if (result == NodeResult.Running)
		{
			return NodeVisitResult.Running;
		}
		switch (m_CurrentExecutionState)
		{
		case ExecutionState.PlanningBranchFinished:
			CurrentSquadTurnControlData.SetReady(m_Agent.Value);
			return NodeVisitResult.GoForward(m_WaitPlanningStageFinished);
		case ExecutionState.PlanningStageFinished:
			return NodeVisitResult.GoForward(SynchronousBranchRoot);
		case ExecutionState.SynchronousBranchFinished:
			m_RuntimeInternalData.Value.EndTurnRequest = true;
			return NodeVisitResult.GoForward(m_WaitSquadMemberTurnStart);
		case ExecutionState.SquadMemberTurnStarted:
			return NodeVisitResult.GoForward(IndividualBranchRoot);
		case ExecutionState.IndividualBranchFinished:
			m_RuntimeInternalData.Value.EndTurnRequest = true;
			if (result != NodeResult.Success)
			{
				return NodeVisitResult.Failure;
			}
			return NodeVisitResult.Success;
		default:
			throw new Exception($"Unknown result: {result}");
		}
	}

	private void TryUpdateExecutionState(NodeResult result)
	{
		if (result != 0)
		{
			m_CurrentExecutionState = m_CurrentExecutionState switch
			{
				ExecutionState.Initial => ExecutionState.PlanningBranchFinished, 
				ExecutionState.PlanningBranchFinished => ExecutionState.PlanningStageFinished, 
				ExecutionState.PlanningStageFinished => ExecutionState.SynchronousBranchFinished, 
				ExecutionState.SynchronousBranchFinished => ExecutionState.SquadMemberTurnStarted, 
				ExecutionState.SquadMemberTurnStarted => ExecutionState.IndividualBranchFinished, 
				_ => m_CurrentExecutionState, 
			};
		}
	}
}
