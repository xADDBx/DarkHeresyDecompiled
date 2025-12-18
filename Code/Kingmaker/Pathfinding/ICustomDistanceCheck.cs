using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface ICustomDistanceCheck
{
	bool IsCloseEnough(GraphNode node);
}
