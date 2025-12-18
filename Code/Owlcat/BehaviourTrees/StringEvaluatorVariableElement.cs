using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/String Evaluator", typeof(string))]
[TypeId("0cc2ad2e579d48a8a1b39f333eb305d8")]
public class StringEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<StringEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new StringEvaluatorVariable(Evaluator);
	}
}
