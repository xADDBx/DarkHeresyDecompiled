namespace Owlcat.BehaviourTrees;

public class SetAbilityNode : ActionNode
{
	private readonly AbilityVariable m_Variable;

	private readonly AbilityVariable m_Value;

	public SetAbilityNode(AbilityVariable variable, AbilityVariable value)
	{
		m_Variable = variable;
		m_Value = value;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_Value.Value;
	}
}
