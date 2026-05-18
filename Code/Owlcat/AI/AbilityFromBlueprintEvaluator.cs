using System;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[ComponentName("AI/AbilityFromBlueprintEvaluator")]
[TypeId("d2bfdb9b01de06040a67c9846f8baa1d")]
public class AbilityFromBlueprintEvaluator : AbilityEvaluator
{
	[SerializeReference]
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	[ValidateNotNull]
	public BlueprintAbilityReference Blueprint;

	public override string GetCaption()
	{
		return $"Ability [{Blueprint}] from [{Entity}]";
	}

	protected override AbilityData GetValueInternal()
	{
		if (!Entity.TryGetValue(out var value))
		{
			return null;
		}
		if (!(value is BaseUnitEntity baseUnitEntity))
		{
			PFLog.AI.Error($"{value} is not BaseUnitEntity");
			return null;
		}
		BlueprintAbility blueprintAbility = Blueprint?.Get();
		if (blueprintAbility == null)
		{
			PFLog.AI.Error("BlueprintAbility is null");
			return null;
		}
		return baseUnitEntity.Abilities.GetAbility(blueprintAbility)?.Data;
	}
}
