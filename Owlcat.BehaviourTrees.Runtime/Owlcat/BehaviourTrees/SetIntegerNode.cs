namespace Owlcat.BehaviourTrees;

public class SetIntegerNode : ActionNode
{
	private readonly IntegerVariable m_Variable;

	private readonly int m_SetValue;

	public SetIntegerNode(IntegerVariable variable, int setValue)
	{
		m_Variable = variable;
		m_SetValue = setValue;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_SetValue;
	}
}
