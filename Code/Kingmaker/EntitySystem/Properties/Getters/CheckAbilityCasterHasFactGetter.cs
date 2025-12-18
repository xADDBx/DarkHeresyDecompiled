using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("626d179bfb8eb2949a501c0f173643e2")]
public class CheckAbilityCasterHasFactGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BlueprintFact CheckedFact;

	protected override int GetBaseValue()
	{
		if (!(this.GetAbility()?.Caster?.Facts.Contains(CheckedFact)).GetValueOrDefault())
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability caster has fact";
	}
}
