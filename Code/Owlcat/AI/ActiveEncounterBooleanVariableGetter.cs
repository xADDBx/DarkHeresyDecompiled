using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/ActiveEncounterBooleanVariableGetter")]
[TypeId("4dd5ff5432d94a5b85ae6a9839cbf124")]
public class ActiveEncounterBooleanVariableGetter : BoolPropertyGetter
{
	[SerializeField]
	private string VariableKey;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "'" + VariableKey + "' from ActiveEncounter";
	}

	protected override bool GetBaseValue()
	{
		return ActiveEncounter.Current?.Blackboard.GetBoolValue(VariableKey) ?? false;
	}
}
