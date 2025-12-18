using System.Collections.Generic;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/GraphNodeList Evaluator", typeof(List<GraphNode>))]
[TypeId("33ee087f3411421aa221bb323d822a18")]
public class GraphNodeListEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<GraphNodeListEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new GraphNodeListEvaluatorVariable(Evaluator);
	}
}
