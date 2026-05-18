using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

public class GraphNodeVariable : BlackboardVariable<GraphNode>
{
	[CanBeNull]
	public override GraphNode Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.AsString() ?? "<null>");
	}
}
