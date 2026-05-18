using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[ComponentName("Equipment/CostOfAbilityWeaponGetter")]
[TypeId("d9a22fd8d3e24f04a52ebc968ef29f65")]
public sealed class CostOfAbilityWeaponGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Cost of ability weapon";
	}

	protected override int GetBaseValue()
	{
		return EvalContext.Current.AbilityWeapon?.Blueprint.Cost ?? 0;
	}
}
