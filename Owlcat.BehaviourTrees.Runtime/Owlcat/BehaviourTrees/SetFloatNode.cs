namespace Owlcat.BehaviourTrees;

public class SetFloatNode : ActionNode
{
	private readonly FloatVariable m_Variable;

	private readonly float m_SetValue;

	public SetFloatNode(FloatVariable variable, float setValue)
	{
		m_Variable = variable;
		m_SetValue = setValue;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_SetValue;
	}
}
