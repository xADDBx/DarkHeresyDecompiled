using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3447705a7dcb46a5b3d56fe74d4101ea")]
public class AlignmentMarkGetter : IntPropertyGetter
{
	public AlignmentAxis AlignmentAxis;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{AlignmentAxis} mark of {FormulaTargetScope.Current}";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		return baseUnitEntity.Alignment.GetAlignmentMark(AlignmentAxis);
	}
}
