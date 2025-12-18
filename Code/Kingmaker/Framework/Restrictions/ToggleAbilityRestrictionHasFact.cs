using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Restrictions;

[Serializable]
[Obsolete]
[ComponentName("Caster Restriction/ToggleAbilityRestrictionHasFact")]
[TypeId("3fa191f10b784ab2bfebccbac45c142e")]
public sealed class ToggleAbilityRestrictionHasFact : BlueprintComponent, IAbilityCasterRestriction
{
	public BpRef<BlueprintUnitFact>[] Facts;

	public bool Inverted;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = false;
		BpRef<BlueprintUnitFact>[] facts = Facts;
		foreach (BpRef<BlueprintUnitFact> bpRef in facts)
		{
			flag = caster.Facts.Contains((BlueprintUnitFact?)bpRef);
			if (flag)
			{
				break;
			}
		}
		return flag != Inverted;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		string text = string.Join(", ", Facts.Select((BpRef<BlueprintUnitFact> i) => i.MaybeBlueprint?.Name).NotNull());
		if (!Inverted)
		{
			return "[HARDCODED] Must have at least one of: " + text;
		}
		return "[HARDCODED] Must not have any of: " + text;
	}
}
