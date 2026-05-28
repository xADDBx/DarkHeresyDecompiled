namespace Owlcat.BehaviourTrees;

public abstract class DecoratorNode : BehaviourTreeNode, IHasChildNode
{
	public BehaviourTreeNode Child { get; set; }

	protected abstract NodeResult Decorate(NodeResult result);

	public override NodeVisitResult ForwardVisit()
	{
		if (Child == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.GoForward(Child);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		return NodeVisitResult.GoBackward(Decorate(result));
	}
}
