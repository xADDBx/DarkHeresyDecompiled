using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("d749f52c649944e8a7e992bc1f3c4387")]
public class IsInPartyGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		return baseUnitEntity.IsInPlayerParty;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + Target.Colorized() + " in player's  party";
	}
}
