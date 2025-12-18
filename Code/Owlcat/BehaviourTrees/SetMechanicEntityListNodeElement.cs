using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/MechanicEntityList/Set MechanicEntityList", "Set MechanicEntityList")]
[TypeId("360364f68445445ba79a58a6aca424c8")]
public class SetMechanicEntityListNodeElement : BehaviourTreeNodeElement<SetMechanicEntityListNode>
{
	public MechanicEntityListVariableReference Variable;

	public MechanicEntityListVariableReference Entities;

	public PropertyCalculator Filter;

	protected override SetMechanicEntityListNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		MechanicEntityListVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		MechanicEntityListVariable runtimeVariable2 = Entities.GetRuntimeVariable(blackboard);
		return new SetMechanicEntityListNode(agentVariable, runtimeVariable, runtimeVariable2, Filter);
	}
}
