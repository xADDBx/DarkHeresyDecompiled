using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("25531d81cf76463caee0b092c8bdfd03")]
public class AlignmentMixGetter : BoolPropertyGetter
{
	public AlignmentMix Mix;

	protected override bool GetBaseValue()
	{
		if (base.CurrentEntity is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.Alignment.GetAlignmentMix() == Mix;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Does {FormulaTargetScope.Current} has mixed alignment {Mix}";
	}
}
