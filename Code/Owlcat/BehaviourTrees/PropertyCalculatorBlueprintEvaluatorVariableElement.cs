using Kingmaker.EntitySystem.Properties;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/PropertyCalculatorBlueprint Evaluator", typeof(PropertyCalculatorBlueprint))]
[TypeId("cfabc2c0827e4a042a86ad2cc4b880c6")]
public class PropertyCalculatorBlueprintEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<PropertyCalculatorBlueprintEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new PropertyCalculatorBlueprintEvaluatorVariable(Evaluator);
	}
}
