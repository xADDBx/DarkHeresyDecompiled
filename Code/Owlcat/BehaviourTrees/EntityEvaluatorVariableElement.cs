using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/MechanicEntity Evaluator", typeof(MechanicEntity))]
[TypeId("02c6fb1c637e4276af85d6775ac66168")]
public class EntityEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<MechanicEntityEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new EntityEvaluatorVariable(Evaluator);
	}
}
