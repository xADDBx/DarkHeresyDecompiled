using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("86b13f19aa70461bb1bda4305b261ffd")]
public class MovedCellsThisTurnGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetCombatStateOptional()?.MovedCellsThisTurn ?? 0f);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " Moved cells this turn";
	}
}
