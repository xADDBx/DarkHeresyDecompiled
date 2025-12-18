using System;
using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.Fmw.Blueprints;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class BodyPartListVariableReference : VariableReference<List<BpRef<BlueprintBodyPart>>>
{
	public BodyPartListVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<BodyPartListVariable>(Id);
	}
}
