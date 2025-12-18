using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/EncounterBlackboard/Set Encounter Integer Value", "Set Encounter Integer Value")]
[TypeId("2e8d0fa32d21447abbd1401d8ac0f5d2")]
public class SetEncounterBlackboardIntegerVariableNodeElement : BehaviourTreeNodeElement<SetEncounterBlackboardIntegerVariableNode>
{
	public string EncounterVariable;

	public PropertyCalculator ValueCalculator;

	protected override SetEncounterBlackboardIntegerVariableNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		EncounterBlackboardVariable encounterBlackboardVariable = blackboard.GetEncounterBlackboardVariable();
		return new SetEncounterBlackboardIntegerVariableNode(agentVariable, encounterBlackboardVariable, EncounterVariable, ValueCalculator);
	}
}
