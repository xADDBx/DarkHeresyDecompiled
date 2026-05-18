using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("b4df6ab66b834c23948e9f581044cb02")]
public sealed class CheckAbilityIsRhymedGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is ability rhymed";
	}

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Ability?.IsRhymed ?? false;
	}
}
