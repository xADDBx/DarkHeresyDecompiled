using System;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic;

[Obsolete("Not needed, PartUnitAlignment does the trick")]
[TypeId("4d0c0980ff194ef991416e047f486152")]
public class BlueprintAlignmentMark : BlueprintFeature
{
	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new AlignmentMark(this, parentContext);
	}
}
