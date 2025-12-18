using System;
using Kingmaker.UnitLogic.Abilities;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class AbilityVariableReference : VariableReference<AbilityData>
{
	public AbilityVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<AbilityVariable>(Id);
	}
}
