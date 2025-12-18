using Kingmaker;
using Kingmaker.EntitySystem.Properties;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SetEncounterBlackboardIntegerVariableNode : BehaviourTreeNode
{
	private readonly EntityVariable m_Agent;

	private readonly EncounterBlackboardVariable m_EncounterBlackboard;

	private readonly string m_EncounterVariableName;

	private readonly PropertyCalculator m_ValueCalculator;

	public SetEncounterBlackboardIntegerVariableNode(EntityVariable agent, EncounterBlackboardVariable encounterBlackboard, string encounterVariableName, PropertyCalculator valueCalculator)
	{
		m_Agent = agent;
		m_EncounterBlackboard = encounterBlackboard;
		m_EncounterVariableName = encounterVariableName;
		m_ValueCalculator = valueCalculator;
	}

	public override NodeVisitResult ForwardVisit()
	{
		if (m_EncounterBlackboard.Value == null)
		{
			PFLog.AI.Error("No EncounterBlackboard found");
			return NodeVisitResult.Failure;
		}
		if (m_ValueCalculator == null || m_ValueCalculator.Empty)
		{
			PFLog.AI.Error("Failed to calculate value");
			return NodeVisitResult.Failure;
		}
		int value = m_ValueCalculator.GetValue(new PropertyContext(m_Agent.Value));
		if (!m_EncounterBlackboard.Value.TrySetIntValue(m_EncounterVariableName, value))
		{
			PFLog.AI.Error("Failed to set variable: " + m_EncounterVariableName);
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}
}
