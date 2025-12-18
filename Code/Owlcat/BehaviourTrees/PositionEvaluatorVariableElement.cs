using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Position Evaluator", typeof(Vector3))]
[TypeId("9bfec8c36c4249b5bede5bb1f03b5308")]
public class PositionEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<PositionEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new PositionEvaluatorVariable(Evaluator);
	}
}
