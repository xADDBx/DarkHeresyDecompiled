using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/Interactable Evaluator", typeof(InteractionAction))]
[TypeId("78c36316440c441e83088d78f9109435")]
public class InteractableEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<InteractionActionEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new InteractableEvaluatorVariable(Evaluator);
	}
}
