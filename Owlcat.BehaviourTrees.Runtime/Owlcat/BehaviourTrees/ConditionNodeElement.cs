namespace Owlcat.BehaviourTrees;

public abstract class ConditionNodeElement<T> : BehaviourTreeNodeElement<T> where T : ConditionNode
{
	public AbortType AbortType;
}
