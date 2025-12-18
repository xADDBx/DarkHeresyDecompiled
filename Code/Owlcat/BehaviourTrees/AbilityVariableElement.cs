using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/Ability", typeof(AbilityData))]
[TypeId("4377f0583ae14d01a199f925dd51ce8f")]
public class AbilityVariableElement : BehaviourTreeVariableElement<AbilityData>
{
	public override BlackboardVariable CreateVariable()
	{
		return new AbilityVariable();
	}
}
