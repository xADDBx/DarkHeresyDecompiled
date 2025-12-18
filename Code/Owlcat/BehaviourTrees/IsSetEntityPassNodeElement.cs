using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Entity/Is Set Entity Pass Node", "Is Set Entity Pass Node")]
[TypeId("e292b04aad074078ac83e4dbbf9f6eed")]
public class IsSetEntityPassNodeElement : ConditionPassNodeElement<IsSetEntityPassNode>
{
	public EntityVariableReference EntityVariable;

	public bool Invert;

	protected override IsSetEntityPassNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = EntityVariable.GetRuntimeVariable(blackboard);
		return new IsSetEntityPassNode(AbortType, runtimeVariable, Invert);
	}
}
