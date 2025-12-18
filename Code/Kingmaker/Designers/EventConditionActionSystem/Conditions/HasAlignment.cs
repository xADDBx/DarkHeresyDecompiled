using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("79cad5d78ebb4235a3c4b8948234161d")]
public class HasAlignment : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public AlignmentAxis Axis;

	public bool CheckByMark;

	public int Value;

	protected override bool CheckCondition()
	{
		if (Axis == AlignmentAxis.None)
		{
			return true;
		}
		if (Value == 0)
		{
			return true;
		}
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (CheckByMark)
		{
			return Value <= baseUnitEntity.Alignment.GetAlignmentMark(Axis);
		}
		return Value <= baseUnitEntity.Alignment.GetAlignmentRank(Axis);
	}

	protected override string GetConditionCaption()
	{
		return string.Format("{0} has {1} in {2} at least {3}", Unit, CheckByMark ? "Mark" : "Rank", Axis, Value);
	}
}
