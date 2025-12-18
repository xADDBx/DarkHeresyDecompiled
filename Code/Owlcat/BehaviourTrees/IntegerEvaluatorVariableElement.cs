using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Integer Evaluator", typeof(int))]
[TypeId("2f6349d8ae7548de85bbb2269394c259")]
public class IntegerEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<IntEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new IntegerEvaluatorVariable(Evaluator);
	}
}
