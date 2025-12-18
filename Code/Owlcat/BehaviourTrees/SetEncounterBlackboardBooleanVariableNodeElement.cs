using Kingmaker.EntitySystem.Properties;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/EncounterBlackboard/Set Encounter Boolean Value", "Set Encounter Boolean Value")]
[TypeId("5dd78371b9da4022b284b08e9546f4bf")]
public class SetEncounterBlackboardBooleanVariableNodeElement : BehaviourTreeNodeElement<SetEncounterBlackboardIntegerVariableNode>
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
