using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SetInteractableNode : ActionNode
{
	private readonly InteractableVariable m_Variable;

	private readonly InteractableVariable m_Value;

	public SetInteractableNode(InteractableVariable variable, InteractableVariable value)
	{
		m_Variable = variable;
		m_Value = value;
	}

	protected override void DoAction()
	{
		m_Variable.Value = m_Value.Value;
	}
}
