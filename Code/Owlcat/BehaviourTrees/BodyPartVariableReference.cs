using System;
using Kingmaker.Code.Gameplay.Blueprints;
using Owlcat.Fmw.Blueprints;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class BodyPartVariableReference : VariableReference<BpRef<BlueprintBodyPart>>
{
	public BodyPartVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<BodyPartVariable>(Id);
	}
}
