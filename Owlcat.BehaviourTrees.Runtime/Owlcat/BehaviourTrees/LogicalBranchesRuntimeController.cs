namespace Owlcat.BehaviourTrees;

public class LogicalBranchesRuntimeController
{
	public void HandleNodeException(BehaviourTreeNode node)
	{
		BehaviourTreeNode behaviourTreeNode = FindLogicalBranchNode(node);
		if (behaviourTreeNode != null)
		{
			behaviourTreeNode.IsDisabled = true;
			BTLog.Error("Logical branch '" + behaviourTreeNode.Title + "' disabled due to an exception, id: '" + behaviourTreeNode.NodeElement.Id + "'");
		}
	}

	private BehaviourTreeNode FindLogicalBranchNode(BehaviourTreeNode node)
	{
		while (node != null)
		{
			if (node.IsLogicalBranch)
			{
				return node;
			}
			node = node.Parent;
		}
		return null;
	}

	public void EnableBranch(BehaviourTreeNode node)
	{
		node.IsDisabled = false;
		node.ErrorException = null;
	}
}
