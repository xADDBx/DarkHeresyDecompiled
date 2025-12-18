using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Obsolete]
[ComponentName("Evaluators/UnitCurrentHPPercent")]
[AllowMultipleComponents]
[TypeId("dd71e86b3083bb745bbee9b444c58aba")]
public class UnitCurrentHPPercent : IntEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override int GetValueInternal()
	{
		PartHealth healthOptional = Unit.GetValue().GetHealthOptional();
		if (healthOptional == null)
		{
			return 100;
		}
		return (int)Math.Floor((float)healthOptional.HitPointsLeft * 100f / (float)healthOptional.MaxHitPoints);
	}

	public override string GetCaption()
	{
		return "Unit current HP percent";
	}
}
