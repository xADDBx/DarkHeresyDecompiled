using System.Collections.Generic;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/PositionList Evaluator", typeof(List<Vector3>))]
[TypeId("42bd42a0df5644f895d030889c78ed89")]
public class PositionListEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<PositionListEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new PositionListEvaluatorVariable(Evaluator);
	}
}
