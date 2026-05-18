using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("965b184cdafe4648a366b7a744196077")]
public sealed class BaseAbilityRangeGetter : IntPropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return EvalContext.Current.Ability.Blueprint.GetBlueprintRange();
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Base ability range";
	}
}
