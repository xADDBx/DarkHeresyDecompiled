using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Entity/Set Entity with Maximum Value", "Set Entity with Maximum Value")]
[TypeId("5cd51286f16649569b3edb938814600b")]
public class SetEntityWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetEntityWithMaximumValueNode>
{
	public EntityVariableReference Variable;

	public MechanicEntityListVariableReference Entities;

	[OptionalParameter]
	public AbilityVariableReference Ability;

	public PropertyCalculator FunctionToMaximize;

	protected override SetEntityWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		EntityVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		MechanicEntityListVariable runtimeVariable2 = Entities.GetRuntimeVariable(blackboard);
		AbilityVariable optionalRuntimeVariable = Ability.GetOptionalRuntimeVariable<AbilityVariable>(blackboard);
		return new SetEntityWithMaximumValueNode(agentVariable, runtimeVariable, runtimeVariable2, optionalRuntimeVariable, FunctionToMaximize);
	}
}
