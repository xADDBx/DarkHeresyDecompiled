using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/BodyPartList/Set BodyPartList", "Set BodyPartList")]
[TypeId("ee07f4f7b9c1402694a87c4d8b8f02a9")]
public class SetBodyPartListNodeElement : BehaviourTreeNodeElement<SetBodyPartListNode>
{
	public BodyPartListVariableReference BodyPartsList;

	public EntityVariableReference Target;

	public PropertyCalculator Filter;

	protected override SetBodyPartListNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		BodyPartListVariable runtimeVariable = BodyPartsList.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable2 = Target.GetRuntimeVariable(blackboard);
		return new SetBodyPartListNode(agentVariable, runtimeVariable, runtimeVariable2, Filter);
	}
}
