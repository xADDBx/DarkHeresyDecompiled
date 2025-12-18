using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Encounter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/ActiveEncounterIntegerVariableGetter")]
[TypeId("3e2f4e979f1646689f7921a9009941d1")]
public class ActiveEncounterIntegerVariableGetter : IntPropertyGetter
{
	[SerializeField]
	private string VariableKey;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "'" + VariableKey + "' from ActiveEncounter";
	}

	protected override int GetBaseValue()
	{
		return ActiveEncounter.Current?.Blackboard.GetIntValue(VariableKey) ?? 0;
	}
}
