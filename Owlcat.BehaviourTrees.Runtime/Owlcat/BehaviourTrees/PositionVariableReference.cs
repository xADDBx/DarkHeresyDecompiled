using System;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class PositionVariableReference : VariableReferenceWithConstant<Vector3>
{
	public PositionVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return GetRuntimeVariable<PositionVariable>(blackboard);
	}
}
