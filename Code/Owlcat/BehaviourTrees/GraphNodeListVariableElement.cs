using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/GraphNodeList", typeof(List<GraphNode>))]
[TypeId("2ef600b8e0084eb2b15c3d976040c7f9")]
public class GraphNodeListVariableElement : BehaviourTreeVariableElement<List<GraphNode>>
{
	public override BlackboardVariable CreateVariable()
	{
		return new GraphNodeListVariable();
	}
}
