using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ecb134acffa147c0bb612a6ffef15a96")]
public class AlignmentRankGetter : IntPropertyGetter
{
	public AlignmentAxis AlignmentAxis;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{AlignmentAxis} rank of {base.CurrentEntity}";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		return baseUnitEntity.Alignment.GetAlignmentRank(AlignmentAxis);
	}
}
