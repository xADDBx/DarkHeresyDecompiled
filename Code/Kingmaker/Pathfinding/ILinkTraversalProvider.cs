using Kingmaker.EntitySystem.Entities;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface ILinkTraversalProvider
{
	MechanicEntity Traverser { get; }

	bool AllowOtherToUseLink { get; }

	bool CanBuildPathThroughLink(GraphNode from, GraphNode to, IWarhammerNodeLink link);
}
