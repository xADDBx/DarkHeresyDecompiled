namespace Owlcat.BehaviourTrees;

public class BooleanConditionPassNode : ConditionPassNode
{
	private readonly BooleanVariable m_Variable;

	private readonly bool m_Invert;

	public BooleanConditionPassNode(AbortType abortType, BooleanVariable variable, bool invert)
		: base(abortType)
	{
		bool invert2 = invert;
		m_Variable = variable;
		m_Invert = invert2;
	}

	public override bool IsPassed()
	{
		return m_Variable.Value ^ m_Invert;
	}
}
