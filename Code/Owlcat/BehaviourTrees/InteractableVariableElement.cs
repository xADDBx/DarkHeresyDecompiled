using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/Interactable", typeof(InteractionAction))]
[TypeId("87c31c5c2c604a11842905240fa56e57")]
public class InteractableVariableElement : BehaviourTreeVariableElement<InteractionAction>
{
	public override BlackboardVariable CreateVariable()
	{
		return new InteractableVariable();
	}
}
