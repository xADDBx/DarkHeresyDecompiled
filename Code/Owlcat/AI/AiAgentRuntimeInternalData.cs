using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Owlcat.BehaviourTrees;
using Pathfinding;

namespace Owlcat.AI;

public class AiAgentRuntimeInternalData
{
	private readonly MechanicEntity m_Agent;

	private readonly IBehaviourTreeProvider m_Provider;

	private readonly BehaviourTreeRuntimeToBlueprintBridge m_RuntimeBridge;

	private bool m_OutOfDate;

	private Task<bool> m_UpdateInternalDataTask = Task.FromException<bool>(new OperationCanceledException());

	public int TurnIndex { get; private set; }

	public AiAreaScanner.PathData AgentMoveVariants { get; private set; }

	public bool EndTurnRequest { get; set; }

	public TimeSpan IdleTime { get; private set; }

	public MechanicEntity Agent => m_Agent;

	public IBehaviourTreeProvider Provider => m_Provider;

	public BehaviourTreeRuntimeToBlueprintBridge RuntimeBridge => m_RuntimeBridge;

	public bool IsReady => m_UpdateInternalDataTask.IsCompleted;

	public AiAgentRuntimeInternalData(BehaviourTreeRuntimeContext runtimeContext, MechanicEntity agent, IParameterizedBehaviourTreeProvider provider)
	{
		m_Agent = agent;
		m_Provider = provider;
		m_RuntimeBridge = runtimeContext.CreateBehaviourTree(provider.ParameterizedBehaviourTree);
	}

	public async Task UpdateMoveVariants(BaseUnitEntity agent, AiThreatsHandlingStrategy threatsHandlingStrategy)
	{
		AgentMoveVariants = await AiAreaScanner.FindAllReachableNodesAsync(agent, agent.Position, agent.GetRequired<PartUnitCombatState>().MovementPoints, threatsHandlingStrategy);
	}

	public void StartNewTurn()
	{
		TurnIndex++;
	}

	public void ResetIdleTime()
	{
		IdleTime = TimeSpan.Zero;
	}

	public void UpdateIdleTime(TimeSpan gameDeltaTimeSpan)
	{
		IdleTime += gameDeltaTimeSpan;
	}

	public void Invalidate()
	{
		m_OutOfDate = true;
	}

	public void TryUpdateInternalData()
	{
		if (m_OutOfDate)
		{
			UpdateInternalData();
		}
	}

	public void UpdateInternalData()
	{
		m_UpdateInternalDataTask = UpdateInternalDataAsync();
	}

	private async Task<bool> UpdateInternalDataAsync()
	{
		m_OutOfDate = false;
		await UpdateMoveVariants((BaseUnitEntity)m_Agent, AiThreatsHandlingStrategy.AvoidIfPossible);
		ResetIdleTime();
		IEnumerable<MechanicEntity> unitsInCombat = Game.Instance.Controllers.TurnController.UnitsInCombat;
		m_RuntimeBridge.SetUnitsInCombatVariable(unitsInCombat.ToList());
		m_RuntimeBridge.SetAlliesInCombatVariable(unitsInCombat.Where((MechanicEntity u) => u != m_Agent && m_Agent.IsAlly(u)).ToList());
		m_RuntimeBridge.SetEnemiesInCombatVariable(unitsInCombat.Where((MechanicEntity u) => u != m_Agent && m_Agent.IsEnemy(u)).ToList());
		m_RuntimeBridge.SetReachableNodesVariable(AgentMoveVariants.cells.Keys.OfType<GridNodeBase>().Where(delegate(GridNodeBase node)
		{
			BaseUnitEntity firstUnit = node.GetFirstUnit();
			return firstUnit == null || firstUnit == m_Agent;
		}));
		return true;
	}
}
