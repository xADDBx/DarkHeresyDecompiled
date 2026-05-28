namespace Owlcat.BehaviourTrees;

public class BooleanConditionNode : ConditionNode
{
	private readonly BooleanVariable m_Variable;

	private readonly bool m_Invert;

	public BooleanConditionNode(AbortType abortType, BooleanVariable variable, bool invert)
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
