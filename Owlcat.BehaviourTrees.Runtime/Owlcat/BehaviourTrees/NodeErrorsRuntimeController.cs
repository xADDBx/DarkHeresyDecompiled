using System;

namespace Owlcat.BehaviourTrees;

public class NodeErrorsRuntimeController
{
	private readonly BehaviourTreeRuntimeContext m_RuntimeContext;

	public NodeErrorsRuntimeController(BehaviourTreeRuntimeContext runtimeContext)
	{
		m_RuntimeContext = runtimeContext;
	}

	public void HandleException(BehaviourTreeNode node, Exception exception)
	{
		node.HasError = true;
		node.ErrorException = exception;
		BTLog.Error($"Node '{node.Title}' failed to visit, id: '{node.NodeElement.Id}', exception: {exception}");
		if (BehaviourTreeConfig.Features.LogicalBranchesEnabled)
		{
			m_RuntimeContext.LogicalBranchesRuntimeController.HandleNodeException(node);
		}
	}

	public void ClearError(BehaviourTreeNode node)
	{
		node.HasError = false;
		node.ErrorException = null;
	}
}
