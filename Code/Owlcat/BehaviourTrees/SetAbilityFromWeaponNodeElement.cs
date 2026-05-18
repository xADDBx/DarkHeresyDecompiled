using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Variables/Ability/Set Ability from Weapon", "Set Ability from Weapon")]
[TypeId("7acf063e3e8a417b8fea37889849b5b5")]
public class SetAbilityFromWeaponNodeElement : BehaviourTreeNodeElement<SetAbilityFromWeaponNode>
{
	public AbilityVariableReference Variable;

	public WeaponHandType WeaponHand;

	public AbilityType AbilityType;

	protected override SetAbilityFromWeaponNode CreateTypedNode(Blackboard blackboard)
	{
		EntityVariable agentVariable = blackboard.GetAgentVariable();
		AbilityVariable runtimeVariable = Variable.GetRuntimeVariable(blackboard);
		return new SetAbilityFromWeaponNode(agentVariable, runtimeVariable, WeaponHand, AbilityType);
	}
}
