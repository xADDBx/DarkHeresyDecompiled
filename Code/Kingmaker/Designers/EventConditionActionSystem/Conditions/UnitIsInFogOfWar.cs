using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[ComponentName("Condition/UnitIsInFogOfWar")]
[AllowMultipleComponents]
[TypeId("25a63cd75c27c3c4c818fa7b5637a9fa")]
public class UnitIsInFogOfWar : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override string GetConditionCaption()
	{
		return $"({Target}) is in fog of war";
	}

	protected override bool CheckCondition()
	{
		return Target.GetValue().IsInFogOfWar;
	}
}
