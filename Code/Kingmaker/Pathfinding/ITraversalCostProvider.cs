using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface ITraversalCostProvider<TIntermediateMetric>
{
	TIntermediateMetric Calc(in TIntermediateMetric distanceFrom, in GraphNode from, in GraphNode to);
}
