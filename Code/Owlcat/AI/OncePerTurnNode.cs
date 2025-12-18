using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class OncePerTurnNode : BlockPassNode
{
	private int m_LastTurnActed;

	private readonly AiAgentRuntimeInternalDataVariable m_RuntimeInternalDataVariable;

	public OncePerTurnNode(AiAgentRuntimeInternalDataVariable runtimeInternalDataVariable, WhenBlockPassRule whenBlockPassRule, ResultInBlockedStateRule resultInBlockedStateRule)
		: base(whenBlockPassRule, resultInBlockedStateRule)
	{
		m_RuntimeInternalDataVariable = runtimeInternalDataVariable;
	}

	public override bool IsStillBlocked()
	{
		return m_LastTurnActed == m_RuntimeInternalDataVariable.Value.TurnIndex;
	}

	protected override void Block()
	{
		m_LastTurnActed = m_RuntimeInternalDataVariable.Value.TurnIndex;
	}
}
