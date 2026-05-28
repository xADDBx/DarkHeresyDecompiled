namespace Owlcat.BehaviourTrees;

public class SubTreeNode : BehaviourTreeNode
{
	public BehaviourTreeRuntimeToBlueprintBridge RuntimeBridge { get; set; }

	public override NodeVisitResult ForwardVisit()
	{
		return NodeVisitResult.GoForward(RuntimeBridge.BehaviourTree.Root);
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		return NodeVisitResult.GoBackward(result);
	}

	public override string ToString()
	{
		return "SubTreeNode: " + RuntimeBridge.BehaviourTreeData.Title;
	}
}
