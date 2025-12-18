using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Squads;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("c29e6b8c4549a2040b4de691f2baa7e8")]
public class CheckUnitIsSquadLeaderGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		PartSquad optional = this.GetTargetByType(Target).GetOptional<PartSquad>();
		if (optional == null || !optional.IsLeader)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " is squad leader";
	}
}
