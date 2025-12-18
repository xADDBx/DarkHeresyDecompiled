using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("13bd257f568b45df9001b1c45c6b8d5f")]
public class CanHaveAlignmentMarkGetter : BoolPropertyGetter
{
	public AlignmentAxis AlignmentAxis;

	public int Mark;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Can {FormulaTargetScope.Current} have {AlignmentAxis} mark {Mark}";
	}

	protected override bool GetBaseValue()
	{
		if (base.CurrentEntity is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.Alignment.CanHaveMarkInAxis(AlignmentAxis, Mark);
		}
		return false;
	}
}
