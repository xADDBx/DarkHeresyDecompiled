using System;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class GraphNodeVariableReference : VariableReference<GraphNode>
{
	public GraphNodeVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<GraphNodeVariable>(Id);
	}
}
