using System;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class FloatVariableReference : VariableReferenceWithConstant<float>
{
	public FloatVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return GetRuntimeVariable<FloatVariable>(blackboard);
	}
}
