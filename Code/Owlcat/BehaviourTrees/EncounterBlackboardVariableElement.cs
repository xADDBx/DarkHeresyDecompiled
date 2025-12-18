using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.EntityBlackboard;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/EncounterBlackboard", typeof(RuntimeEntityBlackboard))]
[TypeId("8125ea5978464f21ae29daee3fbef9b5")]
public class EncounterBlackboardVariableElement : BehaviourTreeVariableElement<RuntimeEntityBlackboard>
{
	[SerializeField]
	private BpRef<BlueprintEncounter> m_Encounter;

	public EncounterBlackboardVariableElement(BpRef<BlueprintEncounter> encounter)
	{
		m_Encounter = encounter;
	}

	public override BlackboardVariable CreateVariable()
	{
		return new EncounterBlackboardVariable(m_Encounter?.MaybeBlueprint);
	}
}
