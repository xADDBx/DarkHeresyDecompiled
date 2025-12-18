using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/BodyPart/Set BodyPart with Maximum Value", "Set BodyPart with Maximum Value")]
[TypeId("4f539bbbd516447a98d84082d237499f")]
public class SetBodyPartWithMaximumValueNodeElement : BehaviourTreeNodeElement<SetBodyPartWithMaximumValueNode>
{
	public BodyPartVariableReference Variable;

	public BodyPartListVariableReference BodyPartsList;

	public EntityVariableReference Target;

	public PropertyCalculator FunctionToMaximize;

	public int MinThresholdValue;

	protected override SetBodyPartWithMaximumValueNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		BodyPartVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		BodyPartListVariable runtimeVariable2 = BodyPartsList.GetRuntimeVariable(blackboard);
		EntityVariable runtimeVariable3 = Target.GetRuntimeVariable(blackboard);
		return new SetBodyPartWithMaximumValueNode(agentVariable, runtimeVariable, runtimeVariable2, runtimeVariable3, FunctionToMaximize, MinThresholdValue);
	}
}
