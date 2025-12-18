namespace Owlcat.BehaviourTrees;

public class SetEntityNode : ActionNode
{
	private readonly EntityVariable m_Variable;

	private readonly EntityVariable m_Value;

	public SetEntityNode(EntityVariable variable, EntityVariable value)
	{
		m_Variable = variable;
		m_Value = value;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_Value.Value;
	}
}
