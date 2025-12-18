using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitIsMovedThisTurn")]
[AllowMultipleComponents]
[TypeId("5388d48db2277394cad64d7626c10400")]
public class UnitIsMovedThisTurn : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override string GetConditionCaption()
	{
		return $"({Unit}) moved this turn";
	}

	protected override bool CheckCondition()
	{
		return Unit.GetValue().GetCombatStateOptional()?.IsMovedThisTurn ?? false;
	}
}
