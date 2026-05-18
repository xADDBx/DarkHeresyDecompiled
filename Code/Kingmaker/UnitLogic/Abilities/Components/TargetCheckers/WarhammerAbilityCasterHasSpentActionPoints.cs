using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("d34868511b5b9a74899446f158d175c1")]
public class WarhammerAbilityCasterHasSpentActionPoints : BlueprintComponent, IAbilityCasterRestriction
{
	public bool not;

	public bool checkMP;

	public bool checkAP;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitCombatState combatStateOptional = caster.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return true;
		}
		bool result = true;
		if (checkMP && combatStateOptional.MovementPoints == (float)combatStateOptional.MovementPointsMax != not)
		{
			result = false;
		}
		if (checkAP && combatStateOptional.ActionPoints == combatStateOptional.ActionPointsMax != not)
		{
			result = false;
		}
		return result;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return ConfigRoot.Instance.LocalizedTexts.Reasons.UnavailableGeneric;
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		return Array.Empty<string>();
	}
}
