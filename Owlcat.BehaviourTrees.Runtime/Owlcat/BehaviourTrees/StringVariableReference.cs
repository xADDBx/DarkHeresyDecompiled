using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class StringVariableReference : VariableReferenceWithConstant<string>
{
	public StringVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return GetRuntimeVariable<StringVariable>(blackboard);
	}
}
