using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[AllowedOn(typeof(BlueprintBehaviourTree))]
[TypeId("5eaa6f08e8d94d579d6228da3c94e0c8")]
public class EncounterBlackboard : BlueprintComponent
{
	[SerializeField]
	private BpRef<BlueprintEncounter> m_Encounter;

	public BlueprintEncounter Encounter => m_Encounter?.MaybeBlueprint;
}
