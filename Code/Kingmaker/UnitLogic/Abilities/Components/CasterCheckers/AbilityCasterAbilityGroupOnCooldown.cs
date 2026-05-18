using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/AbilityCasterAbilityGroupOnCooldown")]
[TypeId("e24ab0051c19c0a429aa51163704840a")]
public class AbilityCasterAbilityGroupOnCooldown : BlueprintComponent, IAbilityCasterRestriction
{
	public bool Not;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = caster.GetAbilityCooldownsOptional()?.GroupIsOnCooldown(AffectedGroup) ?? false;
		if (!Not)
		{
			return flag;
		}
		return !flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.CombatRequired;
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		return Array.Empty<string>();
	}
}
