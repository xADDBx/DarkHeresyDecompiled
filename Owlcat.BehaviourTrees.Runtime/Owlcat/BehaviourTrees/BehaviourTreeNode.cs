using System;

namespace Owlcat.BehaviourTrees;

public abstract class BehaviourTreeNode
{
	public BehaviourTree BehaviourTree { get; set; }

	public BehaviourTreeNode Parent { get; set; }

	public BehaviourTreeNodeElement NodeElement { get; set; }

	public int Depth { get; set; }

	public string Title => NodeElement.Title;

	public bool IsLogicalBranch => NodeElement.IsLogicalBranch;

	public bool IsDisabled { get; set; }

	public bool HasError { get; set; }

	public Exception ErrorException { get; set; }

	public NodeDebugInformation DebugInformation { get; } = new NodeDebugInformation();


	public abstract NodeVisitResult ForwardVisit();

	public virtual NodeVisitResult BackwardVisit(NodeResult result)
	{
		throw new NotSupportedException();
	}

	public override string ToString()
	{
		return Title;
	}
}
