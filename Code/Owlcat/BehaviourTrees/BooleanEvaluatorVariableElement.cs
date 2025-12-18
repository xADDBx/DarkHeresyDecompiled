using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Boolean Evaluator", typeof(bool))]
[TypeId("3fa372e968af467ca0a7e45c9733d833")]
public class BooleanEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<BooleanEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new BooleanEvaluatorVariable(Evaluator);
	}
}
