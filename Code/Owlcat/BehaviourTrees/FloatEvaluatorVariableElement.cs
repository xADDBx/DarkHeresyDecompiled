using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Float Evaluator", typeof(float))]
[TypeId("f787ca80e3934827b56b41cb7d02cfab")]
public class FloatEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<FloatEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new FloatEvaluatorVariable(Evaluator);
	}
}
