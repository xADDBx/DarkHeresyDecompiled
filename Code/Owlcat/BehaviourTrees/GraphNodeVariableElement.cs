using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/GraphNode", typeof(GraphNode))]
[TypeId("aa21aa0f763e4005944ec4abeafdf6d0")]
public class GraphNodeVariableElement : BehaviourTreeVariableElement<GraphNode>
{
	public override BlackboardVariable CreateVariable()
	{
		return new GraphNodeVariable();
	}
}
