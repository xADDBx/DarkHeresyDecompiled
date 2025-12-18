using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/ActiveEncounterIsBooleanVariableSet")]
[TypeId("c5d94843cd4e4c9c8d8b152aae80b718")]
public class ActiveEncounterIsBooleanVariableSet : Condition
{
	[SerializeField]
	private string VariableKey;

	protected override string GetConditionCaption()
	{
		return "ActiveEncounter: is '" + VariableKey + "' set";
	}

	protected override bool CheckCondition()
	{
		return ActiveEncounter.Current?.Blackboard.GetBoolValue(VariableKey) ?? false;
	}
}
