namespace Owlcat.BehaviourTrees;

public class IncreaseIntegerNode : ActionNode
{
	private readonly IntegerVariable m_Variable;

	private readonly IntegerVariable m_IncreaseVariable;

	public IncreaseIntegerNode(IntegerVariable variable, IntegerVariable increaseVariable)
	{
		m_Variable = variable;
		m_IncreaseVariable = increaseVariable;
	}

	protected override void DoAction()
	{
		m_Variable.Value += m_IncreaseVariable.Value;
	}
}
