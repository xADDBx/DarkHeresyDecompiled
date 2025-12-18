using System;
using Kingmaker.EntitySystem.Entities;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class EntityVariableReference : VariableReference<MechanicEntity>
{
	public EntityVariable GetRuntimeVariable(Blackboard blackboard)
	{
		return blackboard.GetVariable<EntityVariable>(Id);
	}
}
