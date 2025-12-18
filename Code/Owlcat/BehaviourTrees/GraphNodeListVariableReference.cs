using System;
using System.Collections.Generic;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class GraphNodeListVariableReference : VariableReference<List<GraphNode>>
{
	public GraphNodeListVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<GraphNodeListVariable>(Id);
	}
}
