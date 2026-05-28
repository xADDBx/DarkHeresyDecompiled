namespace Owlcat.BehaviourTrees;

public class SetBooleanNode : ActionNode
{
	private readonly BooleanVariable m_Variable;

	private readonly bool m_SetValue;

	public SetBooleanNode(BooleanVariable variable, bool setValue)
	{
		m_Variable = variable;
		m_SetValue = setValue;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_SetValue;
	}
}
