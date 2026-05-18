using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Utility.Helpers;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Restrictions;

[Serializable]
[Obsolete]
[ComponentName("Caster Restriction/ToggleAbilityRestrictionStatValue")]
[TypeId("98e5b55a59b1401d880b0ef9b86d86fb")]
public sealed class ToggleAbilityRestrictionStatValue : BlueprintComponent, IAbilityCasterRestriction
{
	public StatType Stat;

	public PropertyCalculator Value;

	public ComparisionType Comparision;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		int value = Value.GetValue(caster);
		return $"[HARDCODED] {Stat} must be {Comparision.GetDescription(value)}";
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		return Array.Empty<string>();
	}
}
