using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("5f00e81fb0604d8a952703ec63ba2b7a")]
public class MovementPointsSpentThisTurnGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return Mathf.RoundToInt(base.CurrentEntity.GetCombatStateOptional()?.MovementPointsSpentThisTurn ?? 0f);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " MP spent this turn";
	}
}
