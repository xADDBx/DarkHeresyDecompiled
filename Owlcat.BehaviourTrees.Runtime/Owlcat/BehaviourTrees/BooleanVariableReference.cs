using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class BooleanVariableReference : VariableReferenceWithConstant<bool>
{
	public BooleanVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return GetRuntimeVariable<BooleanVariable>(blackboard);
	}
}
