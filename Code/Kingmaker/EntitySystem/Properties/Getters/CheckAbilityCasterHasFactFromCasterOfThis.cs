using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("455c9d4ea5184902aae705144c203b68")]
public class CheckAbilityCasterHasFactFromCasterOfThis : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalFact
{
	public BlueprintUnitFactReference FactToCheck;

	protected override bool GetBaseValue()
	{
		IEnumerable<EntityFact> enumerable = this.GetAbility()?.Caster?.Facts.List.Where((EntityFact p) => p.Blueprint == FactToCheck.GetBlueprint());
		MechanicEntity mechanicEntity = this.GetFact()?.MaybeContext?.MaybeCaster;
		if (mechanicEntity == null || enumerable == null || enumerable.Empty())
		{
			return false;
		}
		foreach (EntityFact item in enumerable)
		{
			if (item.MaybeContext?.MaybeCaster == mechanicEntity)
			{
				return true;
			}
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability caster has fact from caster of the fact with the component";
	}
}
