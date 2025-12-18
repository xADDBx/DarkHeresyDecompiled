using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class MechanicEntityListVariableReference : VariableReference<List<MechanicEntity>>
{
	public MechanicEntityListVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<MechanicEntityListVariable>(Id);
	}
}
