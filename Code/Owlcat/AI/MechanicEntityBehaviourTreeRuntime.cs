using System;
using Kingmaker;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class MechanicEntityBehaviourTreeRuntime
{
	private const float TurnTimeoutSeconds = 40f;

	private const float IdleTimeoutSeconds = 3f;

	private static BlueprintCameraFollowSettings CameraFollowSettings = ConfigRoot.Instance.CameraRoot.CameraFollowSettings;

	private static TimeSpan IdleTimeoutTimeSpan = TimeSpan.FromSeconds(3.0);

	public readonly AiAgentRuntimeInternalData RuntimeInternalData;

	private TimeSpan m_TurnStartTimeDelay;

	private TimeSpan m_TurnEndTimeDelay;

	private TimeSpan m_TimeToWaitAtStart;

	private TimeSpan m_TimeToWaitAtEnd;

	private bool m_IsEndTurnRequestProcessed;

	public MechanicEntity Agent => RuntimeInternalData.Agent;

	public IBehaviourTreeProvider Provider => RuntimeInternalData.Provider;

	public BehaviourTreeRuntimeToBlueprintBridge RuntimeBridge => RuntimeInternalData.RuntimeBridge;

	private bool IsReady => RuntimeInternalData.IsReady;

	private bool IsEndTurnRequested => RuntimeInternalData.EndTurnRequest;

	public MechanicEntityBehaviourTreeRuntime(BehaviourTreeRuntimeContext runtimeContext, MechanicEntity agent, IParameterizedBehaviourTreeProvider provider)
	{
		RuntimeInternalData = new AiAgentRuntimeInternalData(runtimeContext, agent, provider);
	}

	public void StartPreparationTask()
	{
		float num = CameraFollowSettings.ToUnitOnStartTurn.BlendSettings.BlendTime + CameraFollowSettings.ToUnitOnStartTurn.CameraObserveTime;
		m_TimeToWaitAtStart = (Agent.IsInSquad ? TimeSpan.Zero : TimeSpan.FromMilliseconds(num * 1000f));
		m_TimeToWaitAtEnd = (Agent.IsInSquad ? TimeSpan.Zero : TimeSpan.FromMilliseconds(500.0));
		m_TurnStartTimeDelay = m_TimeToWaitAtStart;
		m_TurnEndTimeDelay = TimeSpan.FromSeconds(40.0);
		ResetEndTurnRequest();
		StartNewTurn();
		RuntimeInternalData.UpdateInternalData();
	}

	public void Tick(TimeSpan gameDeltaTimeSpan, out bool endTurn)
	{
		endTurn = false;
		m_TurnStartTimeDelay -= gameDeltaTimeSpan;
		m_TurnEndTimeDelay -= gameDeltaTimeSpan;
		RuntimeInternalData.UpdateIdleTime(gameDeltaTimeSpan);
		if (IsEndTurnRequested)
		{
			if (!m_IsEndTurnRequestProcessed)
			{
				m_TurnEndTimeDelay = (Agent.IsInSquad ? TimeSpan.Zero : m_TimeToWaitAtEnd);
				m_IsEndTurnRequestProcessed = true;
			}
			if (m_TurnEndTimeDelay <= TimeSpan.Zero)
			{
				endTurn = true;
			}
			return;
		}
		if (m_TurnEndTimeDelay <= TimeSpan.Zero)
		{
			PFLog.AI.Error("AI-agent acted for too long! Interrupt all commands and end turn");
			endTurn = true;
			return;
		}
		if (IsReady && m_TurnStartTimeDelay <= TimeSpan.Zero)
		{
			using (ContextData<BehaviourTreeContext>.Request().Setup(RuntimeBridge.BehaviourTree.Blackboard))
			{
				RuntimeBridge.BehaviourTree.Tick();
			}
		}
		if (RuntimeInternalData.IdleTime > IdleTimeoutTimeSpan)
		{
			PFLog.AI.Log("End turn, because AI-agent was idled long enough");
			endTurn = true;
		}
		else
		{
			RuntimeInternalData.TryUpdateInternalData();
		}
	}

	private void ResetEndTurnRequest()
	{
		RuntimeInternalData.EndTurnRequest = false;
		m_IsEndTurnRequestProcessed = false;
	}

	private void StartNewTurn()
	{
		RuntimeInternalData.StartNewTurn();
	}

	public void StoreRuntimeState()
	{
		ActiveEncounter.Current?.StoreParticipantRuntimeState(Agent.Ref, new MechanicEntityBehaviourTreeRuntimeState(RuntimeBridge));
	}

	public void RestoreRuntimeState()
	{
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			MechanicEntityBehaviourTreeRuntimeState.Restore(current.GetParticipantRuntimeState(Agent.Ref), RuntimeBridge);
		}
	}
}
