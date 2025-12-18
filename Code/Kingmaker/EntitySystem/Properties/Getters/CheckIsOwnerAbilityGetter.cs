using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f47d622e2c7f4708b8eb5d723af1e2e5")]
public class CheckIsOwnerAbilityGetter : BoolPropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		if (this.GetAbility().Blueprint.OriginalBlueprint == base.Owner)
		{
			return true;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability is from Owner";
	}
}
