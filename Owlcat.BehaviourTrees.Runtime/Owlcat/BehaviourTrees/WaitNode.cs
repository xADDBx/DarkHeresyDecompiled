using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class WaitNode : TaskNode
{
	private readonly FloatVariable m_SecondsVariable;

	public bool IsRunning { get; private set; }

	public float StartTime { get; private set; }

	public WaitNode(FloatVariable secondsVariable)
	{
		m_SecondsVariable = secondsVariable;
	}

	protected override NodeResult OnEnter()
	{
		IsRunning = true;
		StartTime = BehaviourTreeTimeProvider.Time;
		return NodeResult.Running;
	}

	protected override NodeResult OnRunningTick()
	{
		if (BehaviourTreeTimeProvider.Time - StartTime >= m_SecondsVariable.Value)
		{
			return NodeResult.Success;
		}
		return NodeResult.Running;
	}

	protected override void OnExit()
	{
		IsRunning = false;
	}

	public float GetProgress()
	{
		if (!IsRunning)
		{
			return 0f;
		}
		return Mathf.Clamp01((BehaviourTreeTimeProvider.Time - StartTime) / m_SecondsVariable.Value);
	}
}
