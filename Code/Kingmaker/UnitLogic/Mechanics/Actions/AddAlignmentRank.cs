using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("080329fc298d4253b7b255b28b246820")]
public class AddAlignmentRank : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public AlignmentShift Shift;

	public override string GetCaption()
	{
		return $"Adds {Unit} {Shift.Axis} alignment rank {Shift.Value}";
	}

	protected override void RunAction()
	{
		if (Unit.GetValue() is BaseUnitEntity unit)
		{
			AlignmentShiftExtension.ApplyShiftTo(Shift, unit, base.Owner as BlueprintScriptableObject);
		}
	}
}
