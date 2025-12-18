using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Entity/Is Set Entity Leaf", "Is Set Entity")]
[TypeId("f66598aaadc54db7b55c9db8daa937f7")]
public class IsSetEntityNodeElement : ConditionNodeElement<IsSetEntityNode>
{
	public EntityVariableReference EntityVariable;

	public bool Invert;

	protected override IsSetEntityNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable runtimeVariable = EntityVariable.GetRuntimeVariable(blackboard);
		return new IsSetEntityNode(AbortType, runtimeVariable, Invert);
	}
}
