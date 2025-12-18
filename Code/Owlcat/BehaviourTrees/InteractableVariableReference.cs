using System;
using Kingmaker.View.MapObjects;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class InteractableVariableReference : VariableReference<InteractionAction>
{
	public InteractableVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<InteractableVariable>(Id);
	}
}
