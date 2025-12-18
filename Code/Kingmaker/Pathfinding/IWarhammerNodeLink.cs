using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface IWarhammerNodeLink
{
	public delegate void OnTransitionCompletedDel(ILinkTraversalProvider traverser);

	float CostFactor { get; }

	int UnitsWithActivePathTroughLink { get; set; }

	GraphNode StartNode { get; }

	GraphNode EndNode { get; }

	bool IsConnected { get; }

	bool IsActiveAndEnabled { get; }

	event OnTransitionCompletedDel OnTransitionCompleted;

	bool IsInTraverse(ILinkTraversalProvider traverser);

	bool CanStartTraverse(ILinkTraversalProvider traverser);

	void StartTransition(ILinkTraversalProvider traverser);

	void CompleteTransition(ILinkTraversalProvider traverser);

	bool ConnectsNodes(GraphNode from, GraphNode to);
}
