using Kingmaker.UnitLogic.Abilities;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Ability Evaluator", typeof(AbilityData))]
[TypeId("29401b2ac71834f46a83a04f61b5cca8")]
public class AbilityEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<AbilityEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new AbilityEvaluatorVariable(Evaluator);
	}
}
