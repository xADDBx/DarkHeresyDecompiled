namespace Owlcat.BehaviourTrees;

public class RootNode : BehaviourTreeNode, IHasChildNode
{
	public BehaviourTreeNode Child { get; set; }

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
		return NodeVisitResult.GoBackward(result);
	}
}
