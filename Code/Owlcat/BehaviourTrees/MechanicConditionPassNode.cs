using Kingmaker.ElementsSystem;

namespace Owlcat.BehaviourTrees;

public class MechanicConditionPassNode : ConditionPassNode
{
	private readonly ConditionsChecker m_ConditionsChecker;

	public MechanicConditionPassNode(AbortType abortType, ConditionsChecker conditionsChecker)
		: base(abortType)
	{
		m_ConditionsChecker = conditionsChecker;
	}

	public override bool IsPassed()
	{
		return m_ConditionsChecker.Check();
	}
}
