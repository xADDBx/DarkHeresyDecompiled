using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class MechanicConditionNode : ConditionNode
{
	private readonly ConditionsChecker m_ConditionsChecker;

	public MechanicConditionNode(AbortType abortType, ConditionsChecker conditionsChecker)
		: base(abortType)
	{
		m_ConditionsChecker = conditionsChecker;
	}

	public override bool IsPassed()
	{
		return m_ConditionsChecker.Check();
	}
}
