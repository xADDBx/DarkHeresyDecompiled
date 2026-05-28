using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class CooldownNode : BlockPassNode
{
	private readonly FloatVariable m_CooldownVariable;

	private float m_CooldownStartTime;

	public bool IsInCooldown => BehaviourTreeTimeProvider.Time - m_CooldownStartTime < m_CooldownVariable.Value;

	public float CooldownProgress => Mathf.Clamp01((BehaviourTreeTimeProvider.Time - m_CooldownStartTime) / m_CooldownVariable.Value);

	public CooldownNode(FloatVariable cooldownVariable, WhenBlockPassRule whenBlockPassRule, ResultInBlockedStateRule resultInBlockedStateRule)
		: base(whenBlockPassRule, resultInBlockedStateRule)
	{
		m_CooldownVariable = cooldownVariable;
	}

	public override bool IsStillBlocked()
	{
		return IsInCooldown;
	}

	protected override void Block()
	{
		m_CooldownStartTime = BehaviourTreeTimeProvider.Time;
	}
}
