using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("0d381b63e08149ddac7ce6356586c845")]
public class SetAlignmentMark : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public AlignmentAxis Axis;

	public int Mark;

	public override string GetCaption()
	{
		return $"Sets {Unit} {Axis} alignment to {Mark} mark";
	}

	protected override void RunAction()
	{
		if (Unit.GetValue() is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.Alignment.SetMark(Axis, Mark, base.Owner as BlueprintScriptableObject);
		}
	}
}
