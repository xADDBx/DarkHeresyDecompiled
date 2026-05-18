using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class CanInteractConditionNode : ConditionNode
{
	private readonly InteractableVariable m_Interactable;

	public CanInteractConditionNode(AbortType abortType, InteractableVariable interactable)
		: base(abortType)
	{
		m_Interactable = interactable;
	}

	public override bool IsPassed()
	{
		return m_Interactable.Value?.CanInteract() ?? false;
	}
}
