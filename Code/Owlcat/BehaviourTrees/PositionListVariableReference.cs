using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class PositionListVariableReference : VariableReference<List<Vector3>>
{
	public PositionListVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<PositionListVariable>(Id);
	}
}
