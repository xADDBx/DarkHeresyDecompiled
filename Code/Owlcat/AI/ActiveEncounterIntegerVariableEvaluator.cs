using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/ActiveEncounterIntegerVariableEvaluator")]
[TypeId("df4de579fb954c019e08f67a1570ea77")]
public class ActiveEncounterIntegerVariableEvaluator : IntEvaluator
{
	[SerializeField]
	private string VariableKey;

	public override string GetCaption()
	{
		return "'" + VariableKey + "' from ActiveEncounter";
	}

	protected override int GetValueInternal()
	{
		return ActiveEncounter.Current?.Blackboard.GetIntValue(VariableKey) ?? 0;
	}
}
