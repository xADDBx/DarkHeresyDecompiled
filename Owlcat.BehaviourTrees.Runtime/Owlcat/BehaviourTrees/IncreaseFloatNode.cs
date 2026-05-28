namespace Owlcat.BehaviourTrees;

public class IncreaseFloatNode : ActionNode
{
	private readonly FloatVariable m_Variable;

	private readonly FloatVariable m_IncreaseVariable;

	public IncreaseFloatNode(FloatVariable variable, FloatVariable increaseVariable)
	{
		m_Variable = variable;
		m_IncreaseVariable = increaseVariable;
	}

	protected override void DoAction()
	{
		m_Variable.Value += m_IncreaseVariable.Value;
	}
}
