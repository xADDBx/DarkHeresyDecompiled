using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class IntegerVariableReference : VariableReferenceWithConstant<int>
{
	public IntegerVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return GetRuntimeVariable<IntegerVariable>(blackboard);
	}
}
